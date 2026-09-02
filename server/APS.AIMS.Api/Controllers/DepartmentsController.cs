using APS.AIMS.Application.Departments;
using APS.AIMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/departments")]
public sealed class DepartmentsController(
    IDepartmentService departmentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DepartmentDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(
            await departmentService.GetAllAsync(
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DepartmentDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await departmentService.GetByIdAsync(
            id,
            cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("branch/{branchId:guid}")]
    public async Task<ActionResult<IReadOnlyList<DepartmentDto>>> GetByBranch(
        Guid branchId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await departmentService.GetByBranchAsync(
                branchId,
                cancellationToken));
    }

    [Authorize(Roles = AimsAuthorization.CanManageMasterData)]
    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var item = await departmentService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            item);
    }

    [Authorize(Roles = AimsAuthorization.CanManageMasterData)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DepartmentDto>> Update(
        Guid id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var item = await departmentService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [Authorize(Roles = AimsAuthorization.CanManageMasterData)]
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await departmentService.DeactivateAsync(
            id,
            cancellationToken);

        return result ? NoContent() : NotFound();
    }
}
