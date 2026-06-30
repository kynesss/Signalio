using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Signalio.Server.Models;
using Signalio.Server.Services;
using Signalio.Shared.Auth;

namespace Signalio.Server.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);

        return app;
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        JwtTokenService tokenService)
    {
        if (!TryValidate(request, out var validationErrors))
            return Results.ValidationProblem(validationErrors);

        var user = new ApplicationUser { UserName = request.Username };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(
                e => e.Code,
                e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        var token = tokenService.GenerateToken(user);
        return Results.Ok(new AuthResponse { Token = token, Username = user.UserName! });
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        JwtTokenService tokenService)
    {
        if (!TryValidate(request, out var validationErrors))
            return Results.ValidationProblem(validationErrors);

        var user = await userManager.FindByNameAsync(request.Username);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Results.Unauthorized();

        var token = tokenService.GenerateToken(user);
        return Results.Ok(new AuthResponse { Token = token, Username = user.UserName! });
    }

    private static bool TryValidate(
        object model,
        out Dictionary<string, string[]> errors)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        if (Validator.TryValidateObject(model, context, results, validateAllProperties: true))
        {
            errors = new Dictionary<string, string[]>();
            return true;
        }

        errors = results
            .SelectMany(r => r.MemberNames.Select(m => (Member: m, r.ErrorMessage)))
            .GroupBy(x => x.Member, x => x.ErrorMessage ?? string.Empty)
            .ToDictionary(g => g.Key, g => g.ToArray());

        return false;
    }
}
