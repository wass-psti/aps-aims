using APS.AIMS.Application.AssetCategories;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public class AssetCategoryService : IAssetCategoryService
{
    private readonly AimsDbContext _dbContext;

    public AssetCategoryService(AimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AssetCategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssetCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new AssetCategoryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                ParentCategoryId = x.ParentCategoryId,
                ParentCategoryName = x.ParentCategory != null
                    ? x.ParentCategory.Name
                    : null,
                CalibrationRequired = x.CalibrationRequired,
                MaintenanceRequired = x.MaintenanceRequired,
                ApprovalRequired = x.ApprovalRequired,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetCategoryDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssetCategories
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AssetCategoryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                ParentCategoryId = x.ParentCategoryId,
                ParentCategoryName = x.ParentCategory != null
                    ? x.ParentCategory.Name
                    : null,
                CalibrationRequired = x.CalibrationRequired,
                MaintenanceRequired = x.MaintenanceRequired,
                ApprovalRequired = x.ApprovalRequired,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AssetCategoryDto> CreateAsync(
        CreateAssetCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Category code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.");
        }

        var duplicate = await _dbContext.AssetCategories
            .AnyAsync(
                x => x.Code == code,
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Category code '{code}' already exists.");
        }

        AssetCategory? parentCategory = null;

        if (request.ParentCategoryId.HasValue)
        {
            parentCategory = await _dbContext.AssetCategories
                .FirstOrDefaultAsync(
                    x => x.Id == request.ParentCategoryId.Value,
                    cancellationToken);

            if (parentCategory is null)
            {
                throw new ArgumentException(
                    "Selected parent category does not exist.");
            }

            if (!parentCategory.IsActive)
            {
                throw new InvalidOperationException(
                    "Cannot create a category under an inactive parent category.");
            }
        }

        var category = new AssetCategory
        {
            Code = code,
            Name = name,
            Description = request.Description?.Trim(),
            ParentCategoryId = request.ParentCategoryId,
            CalibrationRequired = request.CalibrationRequired,
            MaintenanceRequired = request.MaintenanceRequired,
            ApprovalRequired = request.ApprovalRequired
        };

        _dbContext.AssetCategories.Add(category);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(category, parentCategory?.Name);
    }

    public async Task<AssetCategoryDto?> UpdateAsync(
        Guid id,
        UpdateAssetCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.AssetCategories
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (category is null)
        {
            return null;
        }

        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Category code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.");
        }

        var duplicate = await _dbContext.AssetCategories
            .AnyAsync(
                x => x.Code == code &&
                     x.Id != id,
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Category code '{code}' already exists.");
        }

        if (request.ParentCategoryId == id)
        {
            throw new InvalidOperationException(
                "A category cannot be its own parent.");
        }

        AssetCategory? parentCategory = null;

        if (request.ParentCategoryId.HasValue)
        {
            parentCategory = await _dbContext.AssetCategories
                .FirstOrDefaultAsync(
                    x => x.Id == request.ParentCategoryId.Value,
                    cancellationToken);

            if (parentCategory is null)
            {
                throw new ArgumentException(
                    "Selected parent category does not exist.");
            }

            if (!parentCategory.IsActive)
            {
                throw new InvalidOperationException(
                    "Cannot assign an inactive parent category.");
            }

            await ValidateNoCircularParentAsync(
                id,
                parentCategory.Id,
                cancellationToken);
        }

        category.Code = code;
        category.Name = name;
        category.Description = request.Description?.Trim();
        category.ParentCategoryId = request.ParentCategoryId;
        category.CalibrationRequired = request.CalibrationRequired;
        category.MaintenanceRequired = request.MaintenanceRequired;
        category.ApprovalRequired = request.ApprovalRequired;
        category.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(category, parentCategory?.Name);
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.AssetCategories
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (category is null)
        {
            return false;
        }

        var hasActiveChildren = await _dbContext.AssetCategories
            .AnyAsync(
                x => x.ParentCategoryId == id &&
                     x.IsActive,
                cancellationToken);

        if (hasActiveChildren)
        {
            throw new InvalidOperationException(
                "Deactivate the category's active subcategories first.");
        }

        category.IsActive = false;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidateNoCircularParentAsync(
        Guid categoryId,
        Guid parentCategoryId,
        CancellationToken cancellationToken)
    {
        Guid? currentId = parentCategoryId;

        while (currentId.HasValue)
        {
            if (currentId.Value == categoryId)
            {
                throw new InvalidOperationException(
                    "The selected parent would create a circular category hierarchy.");
            }

            currentId = await _dbContext.AssetCategories
                .Where(x => x.Id == currentId.Value)
                .Select(x => x.ParentCategoryId)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }

    private static AssetCategoryDto Map(
        AssetCategory category,
        string? parentCategoryName)
    {
        return new AssetCategoryDto
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = parentCategoryName,
            CalibrationRequired = category.CalibrationRequired,
            MaintenanceRequired = category.MaintenanceRequired,
            ApprovalRequired = category.ApprovalRequired,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
    }
}