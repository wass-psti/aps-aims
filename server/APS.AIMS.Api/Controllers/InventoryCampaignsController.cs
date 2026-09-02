using APS.AIMS.Application.Inventory;
using APS.AIMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/inventory-campaigns")]
public sealed class InventoryCampaignsController(
    IInventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InventoryCampaignDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(
            await inventoryService.GetCampaignsAsync(
                cancellationToken));
    }

    [Authorize(Roles = AimsAuthorization.CanManageInventory)]
    [HttpPost]
    public async Task<ActionResult<InventoryCampaignDto>> Create(
        CreateInventoryCampaignRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await inventoryService.CreateCampaignAsync(
                request,
                cancellationToken));
    }

    [Authorize(Roles = AimsAuthorization.CanManageInventory)]
    [HttpPost("{campaignId:guid}/start")]
    public async Task<ActionResult<InventoryCampaignDto>> Start(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await inventoryService.StartCampaignAsync(
                campaignId,
                cancellationToken));
    }

    [Authorize(Roles = AimsAuthorization.CanManageInventory)]
    [HttpPost("{campaignId:guid}/complete")]
    public async Task<ActionResult<InventoryCampaignDto>> Complete(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await inventoryService.CompleteCampaignAsync(
                campaignId,
                cancellationToken));
    }

    [HttpGet("{campaignId:guid}/counts")]
    public async Task<ActionResult<IReadOnlyList<InventoryCountDto>>> GetCounts(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await inventoryService.GetCountsAsync(
                campaignId,
                cancellationToken));
    }

    [Authorize(Roles = AimsAuthorization.CanCountInventory)]
    [HttpPost("{campaignId:guid}/counts")]
    public async Task<ActionResult<InventoryCountDto>> RecordCount(
        Guid campaignId,
        RecordInventoryCountRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await inventoryService.RecordCountAsync(
                campaignId,
                request,
                cancellationToken));
    }
}
