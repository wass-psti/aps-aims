namespace APS.AIMS.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? EmployeeNumber { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string? Email { get; set; }

    public Guid? DepartmentId { get; set; }

    public Department? Department { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string DisplayName => $"{FirstName} {LastName}";
}