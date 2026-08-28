using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Application.Assets;

public sealed class AssetDto
{
    public Guid Id { get; init; }

    public required string AssetId { get; init; }

    public required string BarcodeValue { get; init; }

    public required string Name { get; init; }

    public string? ShortDescription { get; init; }

    public Guid CategoryId { get; init; }

    public required string CategoryName { get; init; }

    public string? SerialNumber { get; init; }

    public string? Manufacturer { get; init; }

    public string? Model { get; init; }

    public string? PartNumber { get; init; }

    public string? LegacyAssetId { get; init; }

    public decimal? AcquisitionCost { get; init; }

    public string? Currency { get; init; }

    public Guid CompanyId { get; init; }

    public required string CompanyName { get; init; }

    public Guid BranchId { get; init; }

    public required string BranchName { get; init; }

    public Guid? DepartmentId { get; init; }

    public string? DepartmentName { get; init; }

    public Guid CurrentLocationId { get; init; }

    public required string CurrentLocationName { get; init; }

    public Guid? CurrentCustodianId { get; init; }

    public string? CurrentCustodianName { get; init; }

    public AssetStatus Status { get; init; }

    public AssetCondition Condition { get; init; }

    public bool IsArchived { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}