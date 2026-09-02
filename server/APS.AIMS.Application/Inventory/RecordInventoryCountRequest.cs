using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Application.Inventory;

public sealed class RecordInventoryCountRequest
{
    public required string BarcodeValue { get; init; }

    public Guid ObservedLocationId { get; init; }

    public AssetCondition? ObservedCondition { get; init; }

    public string? Notes { get; init; }
}
