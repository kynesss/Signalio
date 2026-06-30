using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Signalio.Services;

public class TokenAuthenticationStateProvider(TokenStore tokenStore) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStore.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token) || IsExpired(token))
        {
            // Drop a stale/expired token so it doesn't linger in storage.
            if (!string.IsNullOrWhiteSpace(token))
                await tokenStore.RemoveTokenAsync();
            return Anonymous;
        }

        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt", ClaimTypes.Name, ClaimTypes.Role);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task NotifyUserAuthentication(string token)
    {
        await tokenStore.SetTokenAsync(token);
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt", ClaimTypes.Name, ClaimTypes.Role);
        var state = new AuthenticationState(new ClaimsPrincipal(identity));
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    public async Task NotifyUserLogout()
    {
        await tokenStore.RemoveTokenAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private static bool IsExpired(string token)
    {
        var exp = ParseClaimsFromJwt(token).FirstOrDefault(c => c.Type == "exp")?.Value;
        if (long.TryParse(exp, out var seconds))
            return DateTimeOffset.FromUnixTimeSeconds(seconds) <= DateTimeOffset.UtcNow;

        // No exp claim → treat as invalid.
        return true;
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2)
            return [];

        var payload = ParseBase64WithoutPadding(parts[1]);
        var claims = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload);
        if (claims is null)
            return [];

        return claims.Select(kvp => new Claim(MapClaimType(kvp.Key), kvp.Value.ToString()));
    }

    // The server signs tokens with JwtSecurityTokenHandler, whose default outbound
    // map serializes ClaimTypes.Name as "unique_name" and NameIdentifier as "nameid".
    // Map them back so Identity.Name and the usual claim types work client-side.
    private static string MapClaimType(string jwtClaim) => jwtClaim switch
    {
        "unique_name" => ClaimTypes.Name,
        "nameid" => ClaimTypes.NameIdentifier,
        _ => jwtClaim
    };

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
