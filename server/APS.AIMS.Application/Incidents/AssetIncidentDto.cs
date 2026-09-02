using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Application.Incidents;

public sealed class AssetIncidentDto
{
    public Guid Id { get; init; }

    public Guid AssetId { get; init; }

    public required string AssetBusinessId { get; init; }

    public required string AssetName { get; init; }

    public AssetIncidentType Type { get; init; }

    public AssetIncidentSeverity Severity { get; init; }

    public AssetIncidentStatus Status { get; init; }

    public required string Description { get; init; }

    public DateTimeOffset OccurredAt { get; init; }

    public DateTimeOffset ReportedAt { get; init; }

    public string? ResolutionNotes { get; init; }

    public DateTimeOffset? ResolvedAt { get; init; }
}
