namespace APS.AIMS.Application.Assets;

public sealed class UpdateAssetRequest
{
    public required string Name { get; init; }

    public string? ShortDescription { get; init; }

    public Guid CategoryId { get; init; }

    public string? SerialNumber { get; init; }

    public string? Manufacturer { get; init; }

    public string? Model { get; init; }

    public string? PartNumber { get; init; }

    public string? LegacyAssetId { get; init; }

    public decimal? AcquisitionCost { get; init; }

    public string? Currency { get; init; }
}
