using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Application.Assets;

public sealed class CreateAssetRequest
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

    public Guid CompanyId { get; init; }

    public Guid BranchId { get; init; }

    public Guid? DepartmentId { get; init; }

    public Guid CurrentLocationId { get; init; }

    public Guid? CurrentCustodianId { get; init; }

    public string? BarcodeValue { get; init; }

    public AssetStatus Status { get; init; } = AssetStatus.Available;

    public AssetCondition Condition { get; init; } = AssetCondition.New;
}