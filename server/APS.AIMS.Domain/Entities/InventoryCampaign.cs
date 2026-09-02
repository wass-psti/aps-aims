using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Domain.Entities;

public class InventoryCampaign
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public string? Description { get; set; }

    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public InventoryCampaignStatus Status { get; set; } =
        InventoryCampaignStatus.Draft;

    public DateTimeOffset CreatedAt { get; set; } =
        DateTimeOffset.UtcNow;

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}
