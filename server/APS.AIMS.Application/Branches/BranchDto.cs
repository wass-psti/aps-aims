namespace APS.AIMS.Application.Branches;

public class BranchDto
{
    public Guid Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public Guid CompanyId { get; set; }

    public required string CompanyName { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}