using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Domain.Entities;

public class AssetIncident
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public AssetIncidentType Type { get; set; }

    public AssetIncidentSeverity Severity { get; set; }

    public AssetIncidentStatus Status { get; set; } =
        AssetIncidentStatus.Open;

    public required string Description { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset ReportedAt { get; set; } =
        DateTimeOffset.UtcNow;

    public string? ResolutionNotes { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }
}
