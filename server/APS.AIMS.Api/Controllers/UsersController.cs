using APS.AIMS.Application.Users;
using APS.AIMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = AimsRoles.Administrator)]
public sealed class UsersController(
    IUserAdministrationService userService) :
    ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(
            await userService.GetAllAsync(
                cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await userService.CreateAsync(
                request,
                cancellationToken));
    }

    [HttpPut("{userId:guid}")]
    public async Task<ActionResult<UserDto>> Update(
        Guid userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await userService.UpdateAsync(
                userId,
                request,
                cancellationToken));
    }

    [HttpPost("{userId:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        Guid userId,
        ResetUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await userService.ResetPasswordAsync(
            userId,
            request,
            cancellationToken);

        return NoContent();
    }
}
