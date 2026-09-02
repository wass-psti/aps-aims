namespace APS.AIMS.Application.Inventory;

public interface IInventoryService
{
    Task<IReadOnlyList<InventoryCampaignDto>> GetCampaignsAsync(
        CancellationToken cancellationToken = default);

    Task<InventoryCampaignDto> CreateCampaignAsync(
        CreateInventoryCampaignRequest request,
        CancellationToken cancellationToken = default);

    Task<InventoryCampaignDto> StartCampaignAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<InventoryCampaignDto> CompleteCampaignAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InventoryCountDto>> GetCountsAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<InventoryCountDto> RecordCountAsync(
        Guid campaignId,
        RecordInventoryCountRequest request,
        CancellationToken cancellationToken = default);
}
