using APS.AIMS.Application.Companies;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public class CompanyService : ICompanyService
{
    private readonly AimsDbContext _dbContext;

    public CompanyService(AimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CompanyDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Companies
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CompanyDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CompanyDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Companies
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CompanyDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CompanyDto> CreateAsync(
        CreateCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Company code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Company name is required.");
        }

        var duplicateCode = await _dbContext.Companies
            .AnyAsync(x => x.Code == code, cancellationToken);

        if (duplicateCode)
        {
            throw new InvalidOperationException(
                $"Company code '{code}' already exists.");
        }

        var company = new Company
        {
            Code = code,
            Name = name
        };

        _dbContext.Companies.Add(company);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(company);
    }

    public async Task<CompanyDto?> UpdateAsync(
        Guid id,
        UpdateCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (company is null)
        {
            return null;
        }

        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Company code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Company name is required.");
        }

        var duplicateCode = await _dbContext.Companies
            .AnyAsync(
                x => x.Code == code && x.Id != id,
                cancellationToken);

        if (duplicateCode)
        {
            throw new InvalidOperationException(
                $"Company code '{code}' already exists.");
        }

        company.Code = code;
        company.Name = name;
        company.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(company);
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (company is null)
        {
            return false;
        }

        company.IsActive = false;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static CompanyDto Map(Company company)
    {
        return new CompanyDto
        {
            Id = company.Id,
            Code = company.Code,
            Name = company.Name,
            IsActive = company.IsActive,
            CreatedAt = company.CreatedAt
        };
    }
}