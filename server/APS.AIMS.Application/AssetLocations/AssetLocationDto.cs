namespace APS.AIMS.Application.AssetLocations;

public class AssetLocationDto
{
    public Guid Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public Guid BranchId { get; set; }

    public required string BranchName { get; set; }

    public Guid? ParentLocationId { get; set; }

    public string? ParentLocationName { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}