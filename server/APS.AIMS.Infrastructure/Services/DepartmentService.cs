using APS.AIMS.Application.Departments;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public class DepartmentService : IDepartmentService
{
    private readonly AimsDbContext _dbContext;

    public DepartmentService(AimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .AsNoTracking()
            .OrderBy(x => x.Branch.Name)
            .ThenBy(x => x.Name)
            .Select(x => new DepartmentDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                BranchId = x.BranchId,
                BranchName = x.Branch.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DepartmentDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new DepartmentDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                BranchId = x.BranchId,
                BranchName = x.Branch.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetByBranchAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .OrderBy(x => x.Name)
            .Select(x => new DepartmentDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                BranchId = x.BranchId,
                BranchName = x.Branch.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DepartmentDto> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Department code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Department name is required.");
        }

        var branch = await _dbContext.Branches
            .FirstOrDefaultAsync(
                x => x.Id == request.BranchId,
                cancellationToken);

        if (branch is null)
        {
            throw new ArgumentException("Selected branch does not exist.");
        }

        if (!branch.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot create a department under an inactive branch.");
        }

        var duplicateCode = await _dbContext.Departments
            .AnyAsync(
                x => x.BranchId == request.BranchId &&
                     x.Code == code,
                cancellationToken);

        if (duplicateCode)
        {
            throw new InvalidOperationException(
                $"Department code '{code}' already exists for this branch.");
        }

        var department = new Department
        {
            Code = code,
            Name = name,
            BranchId = request.BranchId
        };

        _dbContext.Departments.Add(department);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DepartmentDto
        {
            Id = department.Id,
            Code = department.Code,
            Name = department.Name,
            BranchId = department.BranchId,
            BranchName = branch.Name,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt
        };
    }

    public async Task<DepartmentDto?> UpdateAsync(
        Guid id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (department is null)
        {
            return null;
        }

        var branch = await _dbContext.Branches
            .FirstOrDefaultAsync(
                x => x.Id == request.BranchId,
                cancellationToken);

        if (branch is null)
        {
            throw new ArgumentException("Selected branch does not exist.");
        }

        if (!branch.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot assign a department to an inactive branch.");
        }

        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Department code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Department name is required.");
        }

        var duplicateCode = await _dbContext.Departments
            .AnyAsync(
                x => x.BranchId == request.BranchId &&
                     x.Code == code &&
                     x.Id != id,
                cancellationToken);

        if (duplicateCode)
        {
            throw new InvalidOperationException(
                $"Department code '{code}' already exists for this branch.");
        }

        department.Code = code;
        department.Name = name;
        department.BranchId = request.BranchId;
        department.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DepartmentDto
        {
            Id = department.Id,
            Code = department.Code,
            Name = department.Name,
            BranchId = department.BranchId,
            BranchName = branch.Name,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt
        };
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (department is null)
        {
            return false;
        }

        department.IsActive = false;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}