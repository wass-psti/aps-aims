namespace APS.AIMS.Application.Maintenance;

public sealed class AssetMaintenanceDto
{
    public Guid Id { get; init; }

    public Guid AssetId { get; init; }

    public required string Description { get; init; }

    public string? ServiceProvider { get; init; }

    public string? StartNotes { get; init; }

    public DateTimeOffset StartedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }

    public string? CompletionNotes { get; init; }

    public decimal? Cost { get; init; }

    public string? Currency { get; init; }

    public DateTimeOffset? NextMaintenanceDueAt { get; init; }

    public bool IsOpen => CompletedAt is null;
}
