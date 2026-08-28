namespace APS.AIMS.Application.Departments;

public class CreateDepartmentRequest
{
    public required string Code { get; set; }

    public required string Name { get; set; }

    public Guid BranchId { get; set; }
}