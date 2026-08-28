namespace APS.AIMS.Application.Departments;

public class UpdateDepartmentRequest
{
    public required string Code { get; set; }

    public required string Name { get; set; }

    public Guid BranchId { get; set; }

    public bool IsActive { get; set; }
}