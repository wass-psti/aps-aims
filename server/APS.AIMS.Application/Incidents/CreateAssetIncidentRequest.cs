using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Application.Incidents;

public sealed class CreateAssetIncidentRequest
{
    public Guid AssetId { get; init; }

    public AssetIncidentType Type { get; init; }

    public AssetIncidentSeverity Severity { get; init; }

    public required string Description { get; init; }

    public DateTimeOffset? OccurredAt { get; init; }
}
