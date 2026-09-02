using APS.AIMS.Domain.Security;
using System.Security.Claims;

namespace APS.AIMS.Api.Infrastructure;

public sealed class WorkspaceIdentityMiddleware
{
    private readonly RequestDelegate _next;

    public WorkspaceIdentityMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        /*
         * APS AIMS account management is intentionally disabled when the
         * application runs inside Workspace. Workspace owns the login.
         */
        if (
            context.Request.Path.StartsWithSegments("/api/auth") ||
            context.Request.Path.StartsWithSegments("/api/users")
        )
        {
            context.Response.StatusCode =
                StatusCodes.Status404NotFound;

            return;
        }

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                "00000000-0000-0000-0000-000000000001"),
            new Claim(
                ClaimTypes.Name,
                "Workspace User"),
            new Claim(
                ClaimTypes.Email,
                "workspace@local"),
            new Claim(
                ClaimTypes.Role,
                AimsRoles.Administrator)
        };

        context.User =
            new ClaimsPrincipal(
                new ClaimsIdentity(
                    claims,
                    authenticationType: "Workspace"));

        await _next(context);
    }
}
