namespace APS.AIMS.Application.Companies;

public class UpdateCompanyRequest
{
    public required string Code { get; set; }

    public required string Name { get; set; }

    public bool IsActive { get; set; }
}