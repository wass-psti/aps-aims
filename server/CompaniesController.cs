using APS.AIMS.Application.AssetLocations;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/asset-locations")]
public sealed class AssetLocationsController(
    IAssetLocationService assetLocationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetLocationDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(
            await assetLocationService.GetAllAsync(
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssetLocationDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var location = await assetLocationService.GetByIdAsync(
            id,
            cancellationToken);

        return location is null
            ? NotFound()
            : Ok(location);
    }

    [HttpGet("branch/{branchId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AssetLocationDto>>> GetByBranch(
        Guid branchId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await assetLocationService.GetByBranchAsync(
                branchId,
                cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<AssetLocationDto>> Create(
        CreateAssetLocationRequest request,
        CancellationToken cancellationToken)
    {
        var location = await assetLocationService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = location.Id },
            location);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AssetLocationDto>> Update(
        Guid id,
        UpdateAssetLocationRequest request,
        CancellationToken cancellationToken)
    {
        var location = await assetLocationService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return location is null
            ? NotFound()
            : Ok(location);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await assetLocationService.DeactivateAsync(
            id,
            cancellationToken);

        return result
            ? NoContent()
            : NotFound();
    }
}
