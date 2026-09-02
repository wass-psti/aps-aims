namespace APS.AIMS.Domain.Entities;

public class AssetMaintenance
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public required string Description { get; set; }

    public string? ServiceProvider { get; set; }

    public string? StartNotes { get; set; }

    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }

    public string? CompletionNotes { get; set; }

    public decimal? Cost { get; set; }

    public string? Currency { get; set; }

    public DateTimeOffset? NextMaintenanceDueAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
