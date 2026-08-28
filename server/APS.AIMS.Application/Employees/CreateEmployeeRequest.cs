namespace APS.AIMS.Application.Employees;

public sealed class CreateEmployeeRequest
{
    public string? EmployeeNumber { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? Email { get; init; }

    public Guid? DepartmentId { get; init; }
}
