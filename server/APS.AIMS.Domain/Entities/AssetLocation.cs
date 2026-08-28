namespace APS.AIMS.Domain.Entities;

public class AssetLocation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Code { get; set; }

    public required string Name { get; set; }

    public Guid BranchId { get; set; }

    public Branch Branch { get; set; } = null!;

    public Guid? ParentLocationId { get; set; }

    public AssetLocation? ParentLocation { get; set; }

    public ICollection<AssetLocation> ChildLocations { get; set; } =
        new List<AssetLocation>();

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}