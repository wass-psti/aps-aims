using APS.AIMS.Application.AssetLocations;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/asset-locations")]
public class AssetLocationsController : ControllerBase
{
    private readonly IAssetLocationService _assetLocationService;

    public AssetLocationsController(
        IAssetLocationService assetLocationService)
    {
        _assetLocationService = assetLocationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetLocationDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _assetLocationService.GetAllAsync(
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssetLocationDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var location =
            await _assetLocationService.GetByIdAsync(
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
            await _assetLocationService.GetByBranchAsync(
                branchId,
                cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<AssetLocationDto>> Create(
        CreateAssetLocationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var location =
                await _assetLocationService.CreateAsync(
                    request,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = location.Id },
                location);
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
    public async Task<ActionResult<AssetLocationDto>> Update(
        Guid id,
        UpdateAssetLocationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var location =
                await _assetLocationService.UpdateAsync(
                    id,
                    request,
                    cancellationToken);

            return location is null
                ? NotFound()
                : Ok(location);
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
            await _assetLocationService.DeactivateAsync(
                id,
                cancellationToken);

        return result
            ? NoContent()
            : NotFound();
    }
}