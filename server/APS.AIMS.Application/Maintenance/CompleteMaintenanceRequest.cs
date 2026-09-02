namespace APS.AIMS.Application.Maintenance;

public sealed class CompleteMaintenanceRequest
{
    public string? CompletionNotes { get; init; }

    public decimal? Cost { get; init; }

    public string? Currency { get; init; }

    public DateTimeOffset? NextMaintenanceDueAt { get; init; }
}
