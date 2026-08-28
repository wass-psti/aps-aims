using APS.AIMS.Application.Branches;
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
        return Ok(await branchService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BranchDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var branch = await branchService.GetByIdAsync(id, cancellationToken);

        return branch is null
            ? NotFound()
            : Ok(branch);
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

    [HttpPost]
    public async Task<ActionResult<BranchDto>> Create(
        CreateBranchRequest request,
        CancellationToken cancellationToken)
    {
        var branch = await branchService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = branch.Id },
            branch);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BranchDto>> Update(
        Guid id,
        UpdateBranchRequest request,
        CancellationToken cancellationToken)
    {
        var branch = await branchService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return branch is null
            ? NotFound()
            : Ok(branch);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await branchService.DeactivateAsync(
            id,
            cancellationToken);

        return result
            ? NoContent()
            : NotFound();
    }
}
