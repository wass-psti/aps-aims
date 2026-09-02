using APS.AIMS.Application.Assets;
using APS.AIMS.Application.Maintenance;
using APS.AIMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/assets/{assetId:guid}/maintenance")]
public sealed class AssetMaintenanceController(
    IAssetMaintenanceService maintenanceService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetMaintenanceDto>>> GetHistory(
        Guid assetId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await maintenanceService.GetHistoryAsync(
                assetId,
                cancellationToken));
    }

    [Authorize(Roles = AimsAuthorization.CanManageService)]
    [HttpPost("start")]
    public async Task<ActionResult<AssetDto>> Start(
        Guid assetId,
        StartMaintenanceRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await maintenanceService.StartAsync(
                assetId,
                request,
                cancellationToken));
    }

    [Authorize(Roles = AimsAuthorization.CanManageService)]
    [HttpPost("{maintenanceId:guid}/complete")]
    public async Task<ActionResult<AssetDto>> Complete(
        Guid assetId,
        Guid maintenanceId,
        CompleteMaintenanceRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await maintenanceService.CompleteAsync(
                assetId,
                maintenanceId,
                request,
                cancellationToken));
    }
}
