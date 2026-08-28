namespace APS.AIMS.Application.AssetCategories;

public class CreateAssetCategoryRequest
{
    public required string Code { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public bool CalibrationRequired { get; set; }

    public bool MaintenanceRequired { get; set; }

    public bool ApprovalRequired { get; set; }
}