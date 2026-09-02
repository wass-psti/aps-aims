using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Application.Inventory;

public sealed class InventoryCampaignDto
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public Guid BranchId { get; init; }

    public required string BranchName { get; init; }

    public InventoryCampaignStatus Status { get; init; }

    public int CountedAssets { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }
}
