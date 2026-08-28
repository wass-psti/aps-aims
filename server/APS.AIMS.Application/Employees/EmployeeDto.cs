namespace APS.AIMS.Application.Employees;

public sealed class EmployeeDto
{
    public Guid Id { get; init; }

    public string? EmployeeNumber { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string DisplayName { get; init; }

    public string? Email { get; init; }

    public Guid? DepartmentId { get; init; }

    public string? DepartmentName { get; init; }

    public Guid? BranchId { get; init; }

    public string? BranchName { get; init; }

    public bool IsActive { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
