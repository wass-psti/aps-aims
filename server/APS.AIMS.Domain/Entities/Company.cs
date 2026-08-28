namespace APS.AIMS.Domain.Entities;

public class Company
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Code { get; set; }

    public required string Name { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
}