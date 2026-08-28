using APS.AIMS.Application.Companies;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/companies")]
public sealed class CompaniesController(
    ICompanyService companyService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await companyService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var company = await companyService.GetByIdAsync(id, cancellationToken);

        return company is null
            ? NotFound()
            : Ok(company);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create(
        CreateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var company = await companyService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = company.Id },
            company);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CompanyDto>> Update(
        Guid id,
        UpdateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var company = await companyService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return company is null
            ? NotFound()
            : Ok(company);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await companyService.DeactivateAsync(
            id,
            cancellationToken);

        return result
            ? NoContent()
            : NotFound();
    }
}
