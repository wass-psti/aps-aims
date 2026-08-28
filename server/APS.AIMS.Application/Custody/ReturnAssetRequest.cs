namespace APS.AIMS.Application.Custody;

public sealed class ReturnAssetRequest
{
    public Guid LocationId { get; init; }

    public string? Notes { get; init; }
}
