using Microsoft.JSInterop;

namespace Signalio.Services;

// Thin wrapper over browser localStorage for the single auth token key.
// Shared by TokenAuthenticationStateProvider and AuthorizationMessageHandler.
public class TokenStore(IJSRuntime js)
{
    private const string TokenKey = "authToken";

    public async ValueTask<string?> GetTokenAsync() =>
        await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);

    public async ValueTask SetTokenAsync(string token) =>
        await js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);

    public async ValueTask RemoveTokenAsync() =>
        await js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
}
