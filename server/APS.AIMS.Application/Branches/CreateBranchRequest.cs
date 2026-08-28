namespace APS.AIMS.Application.Branches;

public class CreateBranchRequest
{
    public required string Code { get; set; }

    public required string Name { get; set; }

    public Guid CompanyId { get; set; }
}