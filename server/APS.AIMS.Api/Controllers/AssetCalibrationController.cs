using APS.AIMS.Application.Assets;
using APS.AIMS.Application.Calibration;
using APS.AIMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/assets/{assetId:guid}/calibration")]
public sealed class AssetCalibrationController(
    IAssetCalibrationService calibrationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetCalibrationDto>>> GetHistory(
        Guid assetId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await calibrationService.GetHistoryAsync(
                assetId,
                cancellationToken));
    }

    [Authorize(Roles = AimsAuthorization.CanManageService)]
    [HttpPost("start")]
    public async Task<ActionResult<AssetDto>> Start(
        Guid assetId,
        StartCalibrationRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await calibrationService.StartAsync(
                assetId,
                request,
                cancellationToken));
    }

    [Authorize(Roles = AimsAuthorization.CanManageService)]
    [HttpPost("{calibrationId:guid}/complete")]
    public async Task<ActionResult<AssetDto>> Complete(
        Guid assetId,
        Guid calibrationId,
        CompleteCalibrationRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await calibrationService.CompleteAsync(
                assetId,
                calibrationId,
                request,
                cancellationToken));
    }
}
