using APS.AIMS.Application.Assets;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/assets")]
public sealed class AssetsController(
    IAssetService assetService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(
            await assetService.GetAllAsync(
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssetDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var asset = await assetService.GetByIdAsync(
            id,
            cancellationToken);

        return asset is null
            ? NotFound()
            : Ok(asset);
    }

    [HttpGet("asset-id/{assetId}")]
    public async Task<ActionResult<AssetDto>> GetByAssetId(
        string assetId,
        CancellationToken cancellationToken)
    {
        var asset = await assetService.GetByAssetIdAsync(
            assetId,
            cancellationToken);

        return asset is null
            ? NotFound()
            : Ok(asset);
    }

    [HttpPost]
    public async Task<ActionResult<AssetDto>> Create(
        CreateAssetRequest request,
        CancellationToken cancellationToken)
    {
        var asset = await assetService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = asset.Id },
            asset);
    }
}
