using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Application.Inventory;

public sealed class InventoryCountDto
{
    public Guid Id { get; init; }

    public Guid CampaignId { get; init; }

    public Guid AssetId { get; init; }

    public required string AssetBusinessId { get; init; }

    public required string AssetName { get; init; }

    public required string BarcodeValue { get; init; }

    public Guid SystemLocationId { get; init; }

    public required string SystemLocationName { get; init; }

    public Guid ObservedLocationId { get; init; }

    public required string ObservedLocationName { get; init; }

    public AssetCondition SystemCondition { get; init; }

    public AssetCondition ObservedCondition { get; init; }

    public InventoryCountResult Result { get; init; }

    public string? Notes { get; init; }

    public DateTimeOffset CountedAt { get; init; }
}
