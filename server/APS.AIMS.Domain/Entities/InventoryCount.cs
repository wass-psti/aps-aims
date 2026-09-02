using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Domain.Entities;

public class InventoryCount
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CampaignId { get; set; }
    public InventoryCampaign Campaign { get; set; } = null!;

    public Guid AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public Guid SystemLocationId { get; set; }
    public AssetLocation SystemLocation { get; set; } = null!;

    public Guid ObservedLocationId { get; set; }
    public AssetLocation ObservedLocation { get; set; } = null!;

    public AssetCondition SystemCondition { get; set; }

    public AssetCondition ObservedCondition { get; set; }

    public InventoryCountResult Result { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CountedAt { get; set; } =
        DateTimeOffset.UtcNow;
}
