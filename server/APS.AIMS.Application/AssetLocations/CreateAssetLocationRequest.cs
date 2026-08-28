namespace APS.AIMS.Application.AssetLocations;

public class CreateAssetLocationRequest
{
    public required string Code { get; set; }

    public required string Name { get; set; }

    public Guid BranchId { get; set; }

    public Guid? ParentLocationId { get; set; }
}