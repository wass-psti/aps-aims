using APS.AIMS.Application.Assets;
using APS.AIMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/assets")]
public sealed class AssetsController(
    IAssetService assetService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetAll(
        [FromQuery] AssetFilterRequest filter,
        CancellationToken cancellationToken)
    {
        return Ok(
            await assetService.GetAllAsync(
                filter,
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

        return asset is null ? NotFound() : Ok(asset);
    }

    [HttpGet("asset-id/{assetId}")]
    public async Task<ActionResult<AssetDto>> GetByAssetId(
        string assetId,
        CancellationToken cancellationToken)
    {
        var asset = await assetService.GetByAssetIdAsync(
            assetId,
            cancellationToken);

        return asset is null ? NotFound() : Ok(asset);
    }

    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<AssetDto>> GetByBarcode(
        string barcode,
        CancellationToken cancellationToken)
    {
        var asset = await assetService.GetByBarcodeAsync(
            barcode,
            cancellationToken);

        return asset is null ? NotFound() : Ok(asset);
    }

    [Authorize(Roles = AimsAuthorization.CanManageAssets)]
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

    [Authorize(Roles = AimsAuthorization.CanManageAssets)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AssetDto>> Update(
        Guid id,
        UpdateAssetRequest request,
        CancellationToken cancellationToken)
    {
        var asset = await assetService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return asset is null ? NotFound() : Ok(asset);
    }
}
