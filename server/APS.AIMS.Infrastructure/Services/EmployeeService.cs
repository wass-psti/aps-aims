using APS.AIMS.Application.Common;
using APS.AIMS.Application.Employees;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public sealed class EmployeeService : IEmployeeService
{
    private readonly AimsDbContext _dbContext;

    public EmployeeService(AimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Employees
            .AsNoTracking()
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(employee => employee.IsActive);
        }

        return await query
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .Select(employee => new EmployeeDto
            {
                Id = employee.Id,
                EmployeeNumber = employee.EmployeeNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                DisplayName = employee.FirstName + " " + employee.LastName,
                Email = employee.Email,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department != null
                    ? employee.Department.Name
                    : null,
                BranchId = employee.Department != null
                    ? employee.Department.BranchId
                    : null,
                BranchName = employee.Department != null
                    ? employee.Department.Branch.Name
                    : null,
                IsActive = employee.IsActive,
                CreatedAt = employee.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.Id == id)
            .Select(employee => new EmployeeDto
            {
                Id = employee.Id,
                EmployeeNumber = employee.EmployeeNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                DisplayName = employee.FirstName + " " + employee.LastName,
                Email = employee.Email,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department != null
                    ? employee.Department.Name
                    : null,
                BranchId = employee.Department != null
                    ? employee.Department.BranchId
                    : null,
                BranchName = employee.Department != null
                    ? employee.Department.Branch.Name
                    : null,
                IsActive = employee.IsActive,
                CreatedAt = employee.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EmployeeDto> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var firstName = TextNormalizer.Required(request.FirstName);
        var lastName = TextNormalizer.Required(request.LastName);
        var employeeNumber = TextNormalizer.CodeOrNull(request.EmployeeNumber);
        var email = TextNormalizer.Optional(request.Email);

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name is required.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name is required.");
        }

        if (request.DepartmentId.HasValue)
        {
            var departmentIsValid = await _dbContext.Departments
                .AsNoTracking()
                .AnyAsync(
                    department =>
                        department.Id == request.DepartmentId.Value &&
                        department.IsActive,
                    cancellationToken);

            if (!departmentIsValid)
            {
                throw new ArgumentException(
                    "Selected department does not exist or is inactive.");
            }
        }

        if (employeeNumber is not null)
        {
            var employeeNumberExists = await _dbContext.Employees
                .AsNoTracking()
                .AnyAsync(
                    employee => employee.EmployeeNumber == employeeNumber,
                    cancellationToken);

            if (employeeNumberExists)
            {
                throw new InvalidOperationException(
                    $"Employee number '{employeeNumber}' already exists.");
            }
        }

        var employee = new Employee
        {
            EmployeeNumber = employeeNumber,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            DepartmentId = request.DepartmentId
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(employee.Id, cancellationToken)
            ?? throw new InvalidOperationException(
                "Employee was created but could not be retrieved.");
    }
}
