using APS.AIMS.Application.Assets;
using APS.AIMS.Application.Custody;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/assets/{assetId:guid}/custody")]
public sealed class AssetCustodyController(
    IAssetCustodyService custodyService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetCustodyHistoryDto>>> GetHistory(
        Guid assetId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await custodyService.GetHistoryAsync(
                assetId,
                cancellationToken));
    }

    [HttpPost("issue")]
    public async Task<ActionResult<AssetDto>> Issue(
        Guid assetId,
        IssueAssetRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await custodyService.IssueAsync(
                assetId,
                request,
                cancellationToken));
    }

    [HttpPost("return")]
    public async Task<ActionResult<AssetDto>> Return(
        Guid assetId,
        ReturnAssetRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await custodyService.ReturnAsync(
                assetId,
                request,
                cancellationToken));
    }
}
