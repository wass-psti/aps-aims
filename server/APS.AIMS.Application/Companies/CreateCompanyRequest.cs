namespace APS.AIMS.Application.Companies;

public class CreateCompanyRequest
{
    public required string Code { get; set; }

    public required string Name { get; set; }
}