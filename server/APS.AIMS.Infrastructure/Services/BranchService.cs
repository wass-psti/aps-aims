using APS.AIMS.Application.Branches;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly AimsDbContext _dbContext;

    public BranchService(AimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<BranchDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Branches
            .AsNoTracking()
            .Include(x => x.Company)
            .OrderBy(x => x.Company.Name)
            .ThenBy(x => x.Name)
            .Select(x => new BranchDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                CompanyId = x.CompanyId,
                CompanyName = x.Company.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BranchDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Branches
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new BranchDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                CompanyId = x.CompanyId,
                CompanyName = x.Company.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BranchDto>> GetByCompanyAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Branches
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderBy(x => x.Name)
            .Select(x => new BranchDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                CompanyId = x.CompanyId,
                CompanyName = x.Company.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BranchDto> CreateAsync(
        CreateBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Branch code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Branch name is required.");
        }

        var company = await _dbContext.Companies
            .FirstOrDefaultAsync(
                x => x.Id == request.CompanyId,
                cancellationToken);

        if (company is null)
        {
            throw new ArgumentException("Selected company does not exist.");
        }

        if (!company.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot create a branch under an inactive company.");
        }

        var duplicateCode = await _dbContext.Branches
            .AnyAsync(
                x => x.CompanyId == request.CompanyId &&
                     x.Code == code,
                cancellationToken);

        if (duplicateCode)
        {
            throw new InvalidOperationException(
                $"Branch code '{code}' already exists for this company.");
        }

        var branch = new Branch
        {
            Code = code,
            Name = name,
            CompanyId = request.CompanyId
        };

        _dbContext.Branches.Add(branch);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BranchDto
        {
            Id = branch.Id,
            Code = branch.Code,
            Name = branch.Name,
            CompanyId = branch.CompanyId,
            CompanyName = company.Name,
            IsActive = branch.IsActive,
            CreatedAt = branch.CreatedAt
        };
    }

    public async Task<BranchDto?> UpdateAsync(
        Guid id,
        UpdateBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        var branch = await _dbContext.Branches
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (branch is null)
        {
            return null;
        }

        var company = await _dbContext.Companies
            .FirstOrDefaultAsync(
                x => x.Id == request.CompanyId,
                cancellationToken);

        if (company is null)
        {
            throw new ArgumentException("Selected company does not exist.");
        }

        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Branch code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Branch name is required.");
        }

        var duplicateCode = await _dbContext.Branches
            .AnyAsync(
                x => x.CompanyId == request.CompanyId &&
                     x.Code == code &&
                     x.Id != id,
                cancellationToken);

        if (duplicateCode)
        {
            throw new InvalidOperationException(
                $"Branch code '{code}' already exists for this company.");
        }

        branch.Code = code;
        branch.Name = name;
        branch.CompanyId = request.CompanyId;
        branch.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BranchDto
        {
            Id = branch.Id,
            Code = branch.Code,
            Name = branch.Name,
            CompanyId = branch.CompanyId,
            CompanyName = company.Name,
            IsActive = branch.IsActive,
            CreatedAt = branch.CreatedAt
        };
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var branch = await _dbContext.Branches
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (branch is null)
        {
            return false;
        }

        branch.IsActive = false;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}