using APS.AIMS.Application.Branches;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/branches")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BranchDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _branchService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BranchDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var branch =
            await _branchService.GetByIdAsync(id, cancellationToken);

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
            await _branchService.GetByCompanyAsync(
                companyId,
                cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<BranchDto>> Create(
        CreateBranchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var branch =
                await _branchService.CreateAsync(
                    request,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = branch.Id },
                branch);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BranchDto>> Update(
        Guid id,
        UpdateBranchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var branch =
                await _branchService.UpdateAsync(
                    id,
                    request,
                    cancellationToken);

            return branch is null
                ? NotFound()
                : Ok(branch);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result =
            await _branchService.DeactivateAsync(
                id,
                cancellationToken);

        return result
            ? NoContent()
            : NotFound();
    }
}