using APS.AIMS.Application.AssetLocations;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public class AssetLocationService : IAssetLocationService
{
    private readonly AimsDbContext _dbContext;

    public AssetLocationService(AimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AssetLocationDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssetLocations
            .AsNoTracking()
            .OrderBy(x => x.Branch.Name)
            .ThenBy(x => x.Name)
            .Select(x => new AssetLocationDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                BranchId = x.BranchId,
                BranchName = x.Branch.Name,
                ParentLocationId = x.ParentLocationId,
                ParentLocationName = x.ParentLocation != null
                    ? x.ParentLocation.Name
                    : null,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetLocationDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssetLocations
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AssetLocationDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                BranchId = x.BranchId,
                BranchName = x.Branch.Name,
                ParentLocationId = x.ParentLocationId,
                ParentLocationName = x.ParentLocation != null
                    ? x.ParentLocation.Name
                    : null,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssetLocationDto>> GetByBranchAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssetLocations
            .AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .OrderBy(x => x.Name)
            .Select(x => new AssetLocationDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                BranchId = x.BranchId,
                BranchName = x.Branch.Name,
                ParentLocationId = x.ParentLocationId,
                ParentLocationName = x.ParentLocation != null
                    ? x.ParentLocation.Name
                    : null,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetLocationDto> CreateAsync(
        CreateAssetLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Location code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Location name is required.");
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
                "Cannot create a location under an inactive branch.");
        }

        AssetLocation? parentLocation = null;

        if (request.ParentLocationId.HasValue)
        {
            parentLocation = await _dbContext.AssetLocations
                .FirstOrDefaultAsync(
                    x => x.Id == request.ParentLocationId.Value,
                    cancellationToken);

            if (parentLocation is null)
            {
                throw new ArgumentException(
                    "Selected parent location does not exist.");
            }

            if (parentLocation.BranchId != request.BranchId)
            {
                throw new InvalidOperationException(
                    "Parent location must belong to the same branch.");
            }

            if (!parentLocation.IsActive)
            {
                throw new InvalidOperationException(
                    "Cannot create a location under an inactive parent location.");
            }
        }

        var duplicateCode = await _dbContext.AssetLocations
            .AnyAsync(
                x => x.BranchId == request.BranchId &&
                     x.Code == code,
                cancellationToken);

        if (duplicateCode)
        {
            throw new InvalidOperationException(
                $"Location code '{code}' already exists for this branch.");
        }

        var location = new AssetLocation
        {
            Code = code,
            Name = name,
            BranchId = request.BranchId,
            ParentLocationId = request.ParentLocationId
        };

        _dbContext.AssetLocations.Add(location);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AssetLocationDto
        {
            Id = location.Id,
            Code = location.Code,
            Name = location.Name,
            BranchId = location.BranchId,
            BranchName = branch.Name,
            ParentLocationId = location.ParentLocationId,
            ParentLocationName = parentLocation?.Name,
            IsActive = location.IsActive,
            CreatedAt = location.CreatedAt
        };
    }

    public async Task<AssetLocationDto?> UpdateAsync(
        Guid id,
        UpdateAssetLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var location = await _dbContext.AssetLocations
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (location is null)
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
                "Cannot assign a location to an inactive branch.");
        }

        if (request.ParentLocationId == id)
        {
            throw new InvalidOperationException(
                "A location cannot be its own parent.");
        }

        AssetLocation? parentLocation = null;

        if (request.ParentLocationId.HasValue)
        {
            parentLocation = await _dbContext.AssetLocations
                .FirstOrDefaultAsync(
                    x => x.Id == request.ParentLocationId.Value,
                    cancellationToken);

            if (parentLocation is null)
            {
                throw new ArgumentException(
                    "Selected parent location does not exist.");
            }

            if (parentLocation.BranchId != request.BranchId)
            {
                throw new InvalidOperationException(
                    "Parent location must belong to the same branch.");
            }
        }

        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Location code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Location name is required.");
        }

        var duplicateCode = await _dbContext.AssetLocations
            .AnyAsync(
                x => x.BranchId == request.BranchId &&
                     x.Code == code &&
                     x.Id != id,
                cancellationToken);

        if (duplicateCode)
        {
            throw new InvalidOperationException(
                $"Location code '{code}' already exists for this branch.");
        }

        location.Code = code;
        location.Name = name;
        location.BranchId = request.BranchId;
        location.ParentLocationId = request.ParentLocationId;
        location.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AssetLocationDto
        {
            Id = location.Id,
            Code = location.Code,
            Name = location.Name,
            BranchId = location.BranchId,
            BranchName = branch.Name,
            ParentLocationId = location.ParentLocationId,
            ParentLocationName = parentLocation?.Name,
            IsActive = location.IsActive,
            CreatedAt = location.CreatedAt
        };
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var location = await _dbContext.AssetLocations
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (location is null)
        {
            return false;
        }

        location.IsActive = false;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}