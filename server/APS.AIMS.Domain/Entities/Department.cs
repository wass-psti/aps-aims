namespace APS.AIMS.Domain.Entities;

public class Department
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Code { get; set; }

    public required string Name { get; set; }

    public Guid BranchId { get; set; }

    public Branch Branch { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Employee> Employees { get; set; } =
        new List<Employee>();
}