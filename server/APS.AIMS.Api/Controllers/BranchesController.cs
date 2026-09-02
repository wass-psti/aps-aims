using APS.AIMS.Application.Branches;
using APS.AIMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/branches")]
public sealed class BranchesController(
    IBranchService branchService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BranchDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(
            await branchService.GetAllAsync(
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BranchDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await branchService.GetByIdAsync(
            id,
            cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("company/{companyId:guid}")]
    public async Task<ActionResult<IReadOnlyList<BranchDto>>> GetByCompany(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await branchService.GetByCompanyAsync(
                companyId,
                cancellationToken));
    }

    [Authorize(Roles = AimsAuthorization.CanManageMasterData)]
    [HttpPost]
    public async Task<ActionResult<BranchDto>> Create(
        CreateBranchRequest request,
        CancellationToken cancellationToken)
    {
        var item = await branchService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            item);
    }

    [Authorize(Roles = AimsAuthorization.CanManageMasterData)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BranchDto>> Update(
        Guid id,
        UpdateBranchRequest request,
        CancellationToken cancellationToken)
    {
        var item = await branchService.UpdateAsync(
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
        var result = await branchService.DeactivateAsync(
            id,
            cancellationToken);

        return result ? NoContent() : NotFound();
    }
}
