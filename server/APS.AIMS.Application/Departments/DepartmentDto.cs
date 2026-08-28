namespace APS.AIMS.Application.Departments;

public class DepartmentDto
{
    public Guid Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public Guid BranchId { get; set; }

    public required string BranchName { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}