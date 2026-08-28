namespace APS.AIMS.Domain.Entities;

public class Branch
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Code { get; set; }

    public required string Name { get; set; }

    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Department> Departments { get; set; } =
        new List<Department>();

    public ICollection<AssetLocation> Locations { get; set; } =
        new List<AssetLocation>();
}