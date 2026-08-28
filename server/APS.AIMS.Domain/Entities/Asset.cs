using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Domain.Entities;

public class Asset
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Permanent business Asset ID, e.g. AST-000001
    public required string AssetId { get; set; }

    // QR/barcode value linked to the permanent Asset ID
    public required string BarcodeValue { get; set; }

    public required string Name { get; set; }

    public string? ShortDescription { get; set; }

    public Guid CategoryId { get; set; }

    public AssetCategory Category { get; set; } = null!;

    public string? SerialNumber { get; set; }

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public string? PartNumber { get; set; }

    public string? LegacyAssetId { get; set; }

    public decimal? AcquisitionCost { get; set; }

    public string? Currency { get; set; }

    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public Guid BranchId { get; set; }

    public Branch Branch { get; set; } = null!;

    public Guid? DepartmentId { get; set; }

    public Department? Department { get; set; }

    public Guid CurrentLocationId { get; set; }

    public AssetLocation CurrentLocation { get; set; } = null!;

    public Guid? CurrentCustodianId { get; set; }

    public Employee? CurrentCustodian { get; set; }

    public AssetStatus Status { get; set; } = AssetStatus.Available;

    public AssetCondition Condition { get; set; } = AssetCondition.New;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public bool IsArchived { get; set; }
}