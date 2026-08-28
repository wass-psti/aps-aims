namespace APS.AIMS.Application.AssetLocations;

public class UpdateAssetLocationRequest
{
    public required string Code { get; set; }

    public required string Name { get; set; }

    public Guid BranchId { get; set; }

    public Guid? ParentLocationId { get; set; }

    public bool IsActive { get; set; }
}