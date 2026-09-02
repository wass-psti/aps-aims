using APS.AIMS.Application.Companies;
using APS.AIMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;
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
        return Ok(
            await companyService.GetAllAsync(
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await companyService.GetByIdAsync(
            id,
            cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [Authorize(Roles = AimsAuthorization.CanManageMasterData)]
    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create(
        CreateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var item = await companyService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            item);
    }

    [Authorize(Roles = AimsAuthorization.CanManageMasterData)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CompanyDto>> Update(
        Guid id,
        UpdateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var item = await companyService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [Authorize(Roles = AimsAuthorization.CanManageMasterData)]
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await companyService.DeactivateAsync(
            id,
            cancellationToken);

        return result ? NoContent() : NotFound();
    }
}
