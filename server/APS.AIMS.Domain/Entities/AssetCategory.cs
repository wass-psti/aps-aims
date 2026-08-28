namespace APS.AIMS.Domain.Entities;

public class AssetCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Code { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public AssetCategory? ParentCategory { get; set; }

    public ICollection<AssetCategory> Subcategories { get; set; } =
        new List<AssetCategory>();

    public bool CalibrationRequired { get; set; }

    public bool MaintenanceRequired { get; set; }

    public bool ApprovalRequired { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}