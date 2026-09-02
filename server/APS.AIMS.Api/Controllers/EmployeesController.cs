using APS.AIMS.Application.Employees;
using APS.AIMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/employees")]
public sealed class EmployeesController(
    IEmployeeService employeeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetAll(
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        return Ok(
            await employeeService.GetAllAsync(
                includeInactive,
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByIdAsync(
            id,
            cancellationToken);

        return employee is null ? NotFound() : Ok(employee);
    }

    [Authorize(Roles = AimsAuthorization.CanManageEmployees)]
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = employee.Id },
            employee);
    }

    [Authorize(Roles = AimsAuthorization.CanManageEmployees)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> Update(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return employee is null ? NotFound() : Ok(employee);
    }

    [Authorize(Roles = AimsAuthorization.CanManageEmployees)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await employeeService.DeleteAsync(
            id,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
