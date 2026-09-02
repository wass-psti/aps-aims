using APS.AIMS.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController(
    IAuthenticationService authenticationService) :
    ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await authenticationService.LoginAsync(
                request,
                cancellationToken));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthenticatedUserDto>> Me(
        CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user =
            await authenticationService.GetUserAsync(
                userId,
                cancellationToken);

        return user is null
            ? Unauthorized()
            : Ok(user);
    }
}
