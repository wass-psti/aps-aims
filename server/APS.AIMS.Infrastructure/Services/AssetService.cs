using APS.AIMS.Application.Assets;
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
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Assets
            .AsNoTracking()
            .Where(asset => !asset.IsArchived)
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
        var normalizedAssetId = assetId.Trim().ToUpperInvariant();

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
        ValidateRequest(request);

        await ValidateRelationshipsAsync(
            request,
            cancellationToken);

        var assetId = await GenerateAssetIdAsync(
            cancellationToken);

        var barcodeValue =
            string.IsNullOrWhiteSpace(request.BarcodeValue)
                ? assetId
                : request.BarcodeValue.Trim();

        await ValidateUniqueIdentifiersAsync(
            request.SerialNumber,
            barcodeValue,
            cancellationToken);

        var asset = new Asset
        {
            AssetId = assetId,
            BarcodeValue = barcodeValue,
            Name = request.Name.Trim(),
            ShortDescription = Normalize(request.ShortDescription),

            CategoryId = request.CategoryId,

            SerialNumber = Normalize(request.SerialNumber),
            Manufacturer = Normalize(request.Manufacturer),
            Model = Normalize(request.Model),
            PartNumber = Normalize(request.PartNumber),
            LegacyAssetId = Normalize(request.LegacyAssetId),

            AcquisitionCost = request.AcquisitionCost,
            Currency = NormalizeCurrency(request.Currency),

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

        return await GetByIdAsync(asset.Id, cancellationToken)
            ?? throw new InvalidOperationException(
                "Asset was created but could not be retrieved.");
    }

    private static void ValidateRequest(
        CreateAssetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Asset name is required.");
        }

        if (request.AcquisitionCost < 0)
        {
            throw new ArgumentException(
                "Acquisition cost cannot be negative.");
        }

        if (!string.IsNullOrWhiteSpace(request.Currency) &&
            request.Currency.Trim().Length != 3)
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
            .FirstOrDefaultAsync(
                x => x.Id == request.CompanyId,
                cancellationToken);

        if (company is null || !company.IsActive)
        {
            throw new ArgumentException(
                "Selected company does not exist or is inactive.");
        }

        var branch = await _dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.BranchId,
                cancellationToken);

        if (branch is null ||
            !branch.IsActive ||
            branch.CompanyId != request.CompanyId)
        {
            throw new ArgumentException(
                "Selected branch is invalid for this company.");
        }

        var category = await _dbContext.AssetCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.CategoryId,
                cancellationToken);

        if (category is null || !category.IsActive)
        {
            throw new ArgumentException(
                "Selected asset category does not exist or is inactive.");
        }

        var location = await _dbContext.AssetLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.CurrentLocationId,
                cancellationToken);

        if (location is null ||
            !location.IsActive ||
            location.BranchId != request.BranchId)
        {
            throw new ArgumentException(
                "Selected location is invalid for this branch.");
        }

        if (request.DepartmentId.HasValue)
        {
            var validDepartment =
                await _dbContext.Departments
                    .AsNoTracking()
                    .AnyAsync(
                        x =>
                            x.Id == request.DepartmentId.Value &&
                            x.BranchId == request.BranchId &&
                            x.IsActive,
                        cancellationToken);

            if (!validDepartment)
            {
                throw new ArgumentException(
                    "Selected department is invalid for this branch.");
            }
        }

        if (request.CurrentCustodianId.HasValue)
        {
            var validCustodian =
                await _dbContext.Employees
                    .AsNoTracking()
                    .AnyAsync(
                        x =>
                            x.Id == request.CurrentCustodianId.Value &&
                            x.IsActive,
                        cancellationToken);

            if (!validCustodian)
            {
                throw new ArgumentException(
                    "Selected custodian does not exist or is inactive.");
            }
        }
    }

    private async Task ValidateUniqueIdentifiersAsync(
        string? serialNumber,
        string barcodeValue,
        CancellationToken cancellationToken)
    {
        if (await _dbContext.Assets.AnyAsync(
            x => x.BarcodeValue == barcodeValue,
            cancellationToken))
        {
            throw new InvalidOperationException(
                $"Barcode '{barcodeValue}' is already assigned.");
        }

        var normalizedSerial = Normalize(serialNumber);

        if (normalizedSerial is not null &&
            await _dbContext.Assets.AnyAsync(
                x => x.SerialNumber == normalizedSerial,
                cancellationToken))
        {
            throw new InvalidOperationException(
                $"Serial number '{normalizedSerial}' is already registered.");
        }
    }

    private async Task<string> GenerateAssetIdAsync(
        CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();

        if (connection.State !=
            System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

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

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string? NormalizeCurrency(
        string? currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? null
            : currency.Trim().ToUpperInvariant();
    }
}