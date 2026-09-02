namespace APS.AIMS.Application.Maintenance;

public sealed class StartMaintenanceRequest
{
    public required string Description { get; init; }

    public string? ServiceProvider { get; init; }

    public string? Notes { get; init; }
}
