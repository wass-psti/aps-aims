using APS.AIMS.Application.Assets;
using APS.AIMS.Application.Common;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public sealed class AssetService : IAssetService
{
    private readonly AimsDbContext _dbContext;

    public AssetService(AimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AssetDto>> GetAllAsync(
        AssetFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Assets
            .AsNoTracking()
            .AsQueryable();

        if (!filter.IncludeArchived)
        {
            query = query.Where(asset => !asset.IsArchived);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            var pattern = $"%{search}%";

            query = query.Where(asset =>
                EF.Functions.ILike(asset.AssetId, pattern) ||
                EF.Functions.ILike(asset.BarcodeValue, pattern) ||
                EF.Functions.ILike(asset.Name, pattern) ||
                (asset.SerialNumber != null &&
                 EF.Functions.ILike(asset.SerialNumber, pattern)) ||
                (asset.Manufacturer != null &&
                 EF.Functions.ILike(asset.Manufacturer, pattern)) ||
                (asset.Model != null &&
                 EF.Functions.ILike(asset.Model, pattern)) ||
                (asset.PartNumber != null &&
                 EF.Functions.ILike(asset.PartNumber, pattern)) ||
                (asset.LegacyAssetId != null &&
                 EF.Functions.ILike(asset.LegacyAssetId, pattern)) ||
                EF.Functions.ILike(asset.Category.Name, pattern) ||
                EF.Functions.ILike(asset.Branch.Name, pattern) ||
                EF.Functions.ILike(asset.CurrentLocation.Name, pattern));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(
                asset => asset.CategoryId == filter.CategoryId.Value);
        }

        if (filter.CompanyId.HasValue)
        {
            query = query.Where(
                asset => asset.CompanyId == filter.CompanyId.Value);
        }

        if (filter.BranchId.HasValue)
        {
            query = query.Where(
                asset => asset.BranchId == filter.BranchId.Value);
        }

        if (filter.DepartmentId.HasValue)
        {
            query = query.Where(
                asset => asset.DepartmentId == filter.DepartmentId.Value);
        }

        if (filter.LocationId.HasValue)
        {
            query = query.Where(
                asset => asset.CurrentLocationId == filter.LocationId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(
                asset => asset.Status == filter.Status.Value);
        }

        if (filter.Condition.HasValue)
        {
            query = query.Where(
                asset => asset.Condition == filter.Condition.Value);
        }

        return await query
            .OrderBy(asset => asset.AssetId)
            .Select(AssetProjection.ToDto)
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Assets
            .AsNoTracking()
            .Where(asset => asset.Id == id)
            .Select(AssetProjection.ToDto)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AssetDto?> GetByAssetIdAsync(
        string assetId,
        CancellationToken cancellationToken = default)
    {
        var normalizedAssetId = TextNormalizer.Code(assetId);

        return await _dbContext.Assets
            .AsNoTracking()
            .Where(asset => asset.AssetId == normalizedAssetId)
            .Select(AssetProjection.ToDto)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AssetDto> CreateAsync(
        CreateAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateProfile(
            request.Name,
            request.AcquisitionCost,
            request.Currency);

        await ValidateRelationshipsAsync(
            request,
            cancellationToken);

        var assetId = await GenerateAssetIdAsync(
            cancellationToken);

        var barcodeValue =
            TextNormalizer.Optional(request.BarcodeValue)
            ?? assetId;

        await ValidateUniqueIdentifiersAsync(
            request.SerialNumber,
            barcodeValue,
            null,
            cancellationToken);

        var asset = new Asset
        {
            AssetId = assetId,
            BarcodeValue = barcodeValue,
            Name = TextNormalizer.Required(request.Name),
            ShortDescription = TextNormalizer.Optional(request.ShortDescription),

            CategoryId = request.CategoryId,

            SerialNumber = TextNormalizer.Optional(request.SerialNumber),
            Manufacturer = TextNormalizer.Optional(request.Manufacturer),
            Model = TextNormalizer.Optional(request.Model),
            PartNumber = TextNormalizer.Optional(request.PartNumber),
            LegacyAssetId = TextNormalizer.Optional(request.LegacyAssetId),

            AcquisitionCost = request.AcquisitionCost,
            Currency = TextNormalizer.CodeOrNull(request.Currency),

            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            DepartmentId = request.DepartmentId,
            CurrentLocationId = request.CurrentLocationId,
            CurrentCustodianId = request.CurrentCustodianId,

            Status = request.Status,
            Condition = request.Condition
        };

        _dbContext.Assets.Add(asset);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetRequiredByIdAsync(
            asset.Id,
            cancellationToken);
    }

    public async Task<AssetDto?> UpdateAsync(
        Guid id,
        UpdateAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateProfile(
            request.Name,
            request.AcquisitionCost,
            request.Currency);

        var asset = await _dbContext.Assets
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (asset is null)
        {
            return null;
        }

        await ValidateCategoryAsync(
            request.CategoryId,
            cancellationToken);

        await ValidateUniqueIdentifiersAsync(
            request.SerialNumber,
            null,
            id,
            cancellationToken);

        asset.Name = TextNormalizer.Required(request.Name);
        asset.ShortDescription =
            TextNormalizer.Optional(request.ShortDescription);

        asset.CategoryId = request.CategoryId;

        asset.SerialNumber =
            TextNormalizer.Optional(request.SerialNumber);
        asset.Manufacturer =
            TextNormalizer.Optional(request.Manufacturer);
        asset.Model =
            TextNormalizer.Optional(request.Model);
        asset.PartNumber =
            TextNormalizer.Optional(request.PartNumber);
        asset.LegacyAssetId =
            TextNormalizer.Optional(request.LegacyAssetId);

        asset.AcquisitionCost = request.AcquisitionCost;
        asset.Currency =
            TextNormalizer.CodeOrNull(request.Currency);

        asset.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetRequiredByIdAsync(
            asset.Id,
            cancellationToken);
    }

    private static void ValidateProfile(
        string name,
        decimal? acquisitionCost,
        string? currency)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Asset name is required.");
        }

        if (acquisitionCost < 0)
        {
            throw new ArgumentException(
                "Acquisition cost cannot be negative.");
        }

        var normalizedCurrency =
            TextNormalizer.Optional(currency);

        if (normalizedCurrency is not null &&
            normalizedCurrency.Length != 3)
        {
            throw new ArgumentException(
                "Currency must use a three-letter ISO code.");
        }
    }

    private async Task ValidateRelationshipsAsync(
        CreateAssetRequest request,
        CancellationToken cancellationToken)
    {
        var company = await _dbContext.Companies
            .AsNoTracking()
            .Where(item => item.Id == request.CompanyId)
            .Select(item => new
            {
                item.Id,
                item.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (company is null || !company.IsActive)
        {
            throw new ArgumentException(
                "Selected company does not exist or is inactive.");
        }

        var branch = await _dbContext.Branches
            .AsNoTracking()
            .Where(item => item.Id == request.BranchId)
            .Select(item => new
            {
                item.CompanyId,
                item.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (branch is null ||
            !branch.IsActive ||
            branch.CompanyId != request.CompanyId)
        {
            throw new ArgumentException(
                "Selected branch is invalid for this company.");
        }

        await ValidateCategoryAsync(
            request.CategoryId,
            cancellationToken);

        var locationIsValid = await _dbContext.AssetLocations
            .AsNoTracking()
            .AnyAsync(
                item =>
                    item.Id == request.CurrentLocationId &&
                    item.BranchId == request.BranchId &&
                    item.IsActive,
                cancellationToken);

        if (!locationIsValid)
        {
            throw new ArgumentException(
                "Selected location is invalid for this branch.");
        }

        if (request.DepartmentId.HasValue)
        {
            var departmentIsValid = await _dbContext.Departments
                .AsNoTracking()
                .AnyAsync(
                    item =>
                        item.Id == request.DepartmentId.Value &&
                        item.BranchId == request.BranchId &&
                        item.IsActive,
                    cancellationToken);

            if (!departmentIsValid)
            {
                throw new ArgumentException(
                    "Selected department is invalid for this branch.");
            }
        }

        if (request.CurrentCustodianId.HasValue)
        {
            var custodianIsValid = await _dbContext.Employees
                .AsNoTracking()
                .AnyAsync(
                    item =>
                        item.Id == request.CurrentCustodianId.Value &&
                        item.IsActive,
                    cancellationToken);

            if (!custodianIsValid)
            {
                throw new ArgumentException(
                    "Selected custodian does not exist or is inactive.");
            }
        }
    }

    private async Task ValidateCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var categoryIsValid = await _dbContext.AssetCategories
            .AsNoTracking()
            .AnyAsync(
                item =>
                    item.Id == categoryId &&
                    item.IsActive,
                cancellationToken);

        if (!categoryIsValid)
        {
            throw new ArgumentException(
                "Selected asset category does not exist or is inactive.");
        }
    }

    private async Task ValidateUniqueIdentifiersAsync(
        string? serialNumber,
        string? barcodeValue,
        Guid? excludedAssetId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(barcodeValue))
        {
            var normalizedBarcode =
                TextNormalizer.Required(barcodeValue);

            var barcodeExists = await _dbContext.Assets
                .AsNoTracking()
                .AnyAsync(
                    item =>
                        item.BarcodeValue == normalizedBarcode &&
                        (!excludedAssetId.HasValue ||
                         item.Id != excludedAssetId.Value),
                    cancellationToken);

            if (barcodeExists)
            {
                throw new InvalidOperationException(
                    $"Barcode '{normalizedBarcode}' is already assigned.");
            }
        }

        var normalizedSerial =
            TextNormalizer.Optional(serialNumber);

        if (normalizedSerial is null)
        {
            return;
        }

        var serialExists = await _dbContext.Assets
            .AsNoTracking()
            .AnyAsync(
                item =>
                    item.SerialNumber == normalizedSerial &&
                    (!excludedAssetId.HasValue ||
                     item.Id != excludedAssetId.Value),
                cancellationToken);

        if (serialExists)
        {
            throw new InvalidOperationException(
                $"Serial number '{normalizedSerial}' is already registered.");
        }
    }

    private async Task<AssetDto> GetRequiredByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException(
                "Asset was saved but could not be retrieved.");
    }

    private async Task<string> GenerateAssetIdAsync(
        CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var shouldClose =
            connection.State !=
            System.Data.ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command =
                connection.CreateCommand();

            command.CommandText =
                """SELECT nextval('"AssetIdSequence"');""";

            var result =
                await command.ExecuteScalarAsync(
                    cancellationToken);

            var sequence =
                Convert.ToInt64(result);

            return $"AST-{sequence:D6}";
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}
