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

        return await Project(query)
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Project(
                _dbContext.Employees
                    .AsNoTracking()
                    .Where(employee => employee.Id == id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EmployeeDto> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalized = await ValidateAndNormalizeAsync(
            request.EmployeeNumber,
            request.FirstName,
            request.LastName,
            request.Email,
            request.DepartmentId,
            excludedEmployeeId: null,
            cancellationToken);

        var employee = new Employee
        {
            EmployeeNumber = normalized.EmployeeNumber,
            FirstName = normalized.FirstName,
            LastName = normalized.LastName,
            Email = normalized.Email,
            DepartmentId = request.DepartmentId,
            IsActive = true
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetRequiredAsync(employee.Id, cancellationToken);
    }

    public async Task<EmployeeDto?> UpdateAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var employee = await _dbContext.Employees
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (employee is null)
        {
            return null;
        }

        var normalized = await ValidateAndNormalizeAsync(
            request.EmployeeNumber,
            request.FirstName,
            request.LastName,
            request.Email,
            request.DepartmentId,
            id,
            cancellationToken);

        if (!request.IsActive)
        {
            var isCurrentCustodian = await _dbContext.Assets
                .AsNoTracking()
                .AnyAsync(
                    asset => asset.CurrentCustodianId == id,
                    cancellationToken);

            if (isCurrentCustodian)
            {
                throw new InvalidOperationException(
                    "Return all assets currently assigned to this employee before deactivating the employee.");
            }
        }

        employee.EmployeeNumber = normalized.EmployeeNumber;
        employee.FirstName = normalized.FirstName;
        employee.LastName = normalized.LastName;
        employee.Email = normalized.Email;
        employee.DepartmentId = request.DepartmentId;
        employee.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetRequiredAsync(employee.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var employee = await _dbContext.Employees
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (employee is null)
        {
            return false;
        }

        var hasCurrentAsset = await _dbContext.Assets
            .AsNoTracking()
            .AnyAsync(
                asset => asset.CurrentCustodianId == id,
                cancellationToken);

        if (hasCurrentAsset)
        {
            throw new InvalidOperationException(
                "This employee currently holds an asset and cannot be permanently deleted. Return the asset first.");
        }

        var hasCustodyHistory = await _dbContext.AssetCustodyHistories
            .AsNoTracking()
            .AnyAsync(
                history => history.EmployeeId == id,
                cancellationToken);

        var hasTransactionHistory = await _dbContext.AssetTransactions
            .AsNoTracking()
            .AnyAsync(
                transaction =>
                    transaction.FromCustodianId == id ||
                    transaction.ToCustodianId == id,
                cancellationToken);

        if (hasCustodyHistory || hasTransactionHistory)
        {
            throw new InvalidOperationException(
                "This employee is referenced by asset history and cannot be permanently deleted without breaking audit records. Mark the employee inactive instead.");
        }

        _dbContext.Employees.Remove(employee);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<NormalizedEmployee> ValidateAndNormalizeAsync(
        string? employeeNumber,
        string firstName,
        string lastName,
        string? email,
        Guid? departmentId,
        Guid? excludedEmployeeId,
        CancellationToken cancellationToken)
    {
        var normalizedFirstName = TextNormalizer.Required(firstName);
        var normalizedLastName = TextNormalizer.Required(lastName);
        var normalizedEmployeeNumber =
            TextNormalizer.CodeOrNull(employeeNumber);
        var normalizedEmail = TextNormalizer.Optional(email);

        if (string.IsNullOrWhiteSpace(normalizedFirstName))
        {
            throw new ArgumentException("First name is required.");
        }

        if (string.IsNullOrWhiteSpace(normalizedLastName))
        {
            throw new ArgumentException("Last name is required.");
        }

        if (departmentId.HasValue)
        {
            var departmentIsValid = await _dbContext.Departments
                .AsNoTracking()
                .AnyAsync(
                    department =>
                        department.Id == departmentId.Value &&
                        department.IsActive,
                    cancellationToken);

            if (!departmentIsValid)
            {
                throw new ArgumentException(
                    "Selected department does not exist or is inactive.");
            }
        }

        if (normalizedEmployeeNumber is not null)
        {
            var duplicateExists = await _dbContext.Employees
                .AsNoTracking()
                .AnyAsync(
                    employee =>
                        employee.EmployeeNumber == normalizedEmployeeNumber &&
                        (!excludedEmployeeId.HasValue ||
                         employee.Id != excludedEmployeeId.Value),
                    cancellationToken);

            if (duplicateExists)
            {
                throw new InvalidOperationException(
                    $"Employee number '{normalizedEmployeeNumber}' already exists.");
            }
        }

        return new NormalizedEmployee(
            normalizedEmployeeNumber,
            normalizedFirstName,
            normalizedLastName,
            normalizedEmail);
    }

    private async Task<EmployeeDto> GetRequiredAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException(
                "Employee was saved but could not be retrieved.");
    }

    private static IQueryable<EmployeeDto> Project(
        IQueryable<Employee> query)
    {
        return query.Select(employee => new EmployeeDto
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
            CompanyId = employee.Department != null
                ? employee.Department.Branch.CompanyId
                : null,
            CompanyName = employee.Department != null
                ? employee.Department.Branch.Company.Name
                : null,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt
        });
    }

    private sealed record NormalizedEmployee(
        string? EmployeeNumber,
        string FirstName,
        string LastName,
        string? Email);
}
