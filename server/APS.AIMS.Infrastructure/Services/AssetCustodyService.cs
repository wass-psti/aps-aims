using APS.AIMS.Application.Assets;
using APS.AIMS.Application.Common;
using APS.AIMS.Application.Custody;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Domain.Enums;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public sealed class AssetCustodyService : IAssetCustodyService
{
    private readonly AimsDbContext _dbContext;
    private readonly IAssetService _assetService;

    public AssetCustodyService(
        AimsDbContext dbContext,
        IAssetService assetService)
    {
        _dbContext = dbContext;
        _assetService = assetService;
    }

    public async Task<IReadOnlyList<AssetCustodyHistoryDto>> GetHistoryAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssetCustodyHistories
            .AsNoTracking()
            .Where(history => history.AssetId == assetId)
            .OrderByDescending(history => history.IssuedAt)
            .Select(history => new AssetCustodyHistoryDto
            {
                Id = history.Id,
                AssetId = history.AssetId,
                EmployeeId = history.EmployeeId,
                EmployeeName =
                    history.Employee.FirstName + " " +
                    history.Employee.LastName,
                EmployeeNumber = history.Employee.EmployeeNumber,
                IssuedFromLocationId = history.IssuedFromLocationId,
                IssuedFromLocationName = history.IssuedFromLocation.Name,
                ReturnedToLocationId = history.ReturnedToLocationId,
                ReturnedToLocationName = history.ReturnedToLocation != null
                    ? history.ReturnedToLocation.Name
                    : null,
                IssuedAt = history.IssuedAt,
                ReturnedAt = history.ReturnedAt,
                IssueNotes = history.IssueNotes,
                ReturnNotes = history.ReturnNotes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetDto> IssueAsync(
        Guid assetId,
        IssueAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var asset = await GetAssetForUpdateAsync(
            assetId,
            cancellationToken);

        if (asset is null)
        {
            throw new KeyNotFoundException("Asset was not found.");
        }

        if (asset.IsArchived)
        {
            throw new InvalidOperationException(
                "Archived assets cannot be issued.");
        }

        if (asset.Status != AssetStatus.Available ||
            asset.CurrentCustodianId.HasValue)
        {
            throw new InvalidOperationException(
                "Only available assets without a current custodian can be issued.");
        }

        var employee = await _dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == request.EmployeeId,
                cancellationToken);

        if (employee is null || !employee.IsActive)
        {
            throw new ArgumentException(
                "Selected employee does not exist or is inactive.");
        }

        var now = DateTimeOffset.UtcNow;
        var notes = TextNormalizer.Optional(request.Notes);

        var custody = new AssetCustodyHistory
        {
            AssetId = asset.Id,
            EmployeeId = employee.Id,
            IssuedFromLocationId = asset.CurrentLocationId,
            IssuedAt = now,
            IssueNotes = notes
        };

        var transactionRecord = new AssetTransaction
        {
            AssetId = asset.Id,
            Type = AssetTransactionType.Issue,
            FromCustodianId = asset.CurrentCustodianId,
            ToCustodianId = employee.Id,
            FromLocationId = asset.CurrentLocationId,
            ToLocationId = asset.CurrentLocationId,
            FromStatus = asset.Status,
            ToStatus = AssetStatus.Issued,
            Notes = notes,
            OccurredAt = now
        };

        asset.CurrentCustodianId = employee.Id;
        asset.Status = AssetStatus.Issued;
        asset.UpdatedAt = now;

        _dbContext.AssetCustodyHistories.Add(custody);
        _dbContext.AssetTransactions.Add(transactionRecord);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetRequiredAssetAsync(asset.Id, cancellationToken);
    }

    public async Task<AssetDto> ReturnAsync(
        Guid assetId,
        ReturnAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var asset = await GetAssetForUpdateAsync(
            assetId,
            cancellationToken);

        if (asset is null)
        {
            throw new KeyNotFoundException("Asset was not found.");
        }

        if (asset.Status != AssetStatus.Issued ||
            !asset.CurrentCustodianId.HasValue)
        {
            throw new InvalidOperationException(
                "Only currently issued assets can be returned.");
        }

        var returnLocation = await _dbContext.AssetLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                location =>
                    location.Id == request.LocationId &&
                    location.BranchId == asset.BranchId &&
                    location.IsActive,
                cancellationToken);

        if (returnLocation is null)
        {
            throw new ArgumentException(
                "Selected return location is invalid for this asset's branch.");
        }

        var custody = await _dbContext.AssetCustodyHistories
            .FirstOrDefaultAsync(
                history =>
                    history.AssetId == asset.Id &&
                    history.ReturnedAt == null,
                cancellationToken);

        if (custody is null)
        {
            throw new InvalidOperationException(
                "The asset has no open custody record to close.");
        }

        var now = DateTimeOffset.UtcNow;
        var notes = TextNormalizer.Optional(request.Notes);
        var previousCustodianId = asset.CurrentCustodianId;
        var previousLocationId = asset.CurrentLocationId;
        var previousStatus = asset.Status;

        custody.ReturnedAt = now;
        custody.ReturnedToLocationId = returnLocation.Id;
        custody.ReturnNotes = notes;

        var transactionRecord = new AssetTransaction
        {
            AssetId = asset.Id,
            Type = AssetTransactionType.Return,
            FromCustodianId = previousCustodianId,
            ToCustodianId = null,
            FromLocationId = previousLocationId,
            ToLocationId = returnLocation.Id,
            FromStatus = previousStatus,
            ToStatus = AssetStatus.Available,
            Notes = notes,
            OccurredAt = now
        };

        asset.CurrentCustodianId = null;
        asset.CurrentLocationId = returnLocation.Id;
        asset.Status = AssetStatus.Available;
        asset.UpdatedAt = now;

        _dbContext.AssetTransactions.Add(transactionRecord);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetRequiredAssetAsync(asset.Id, cancellationToken);
    }

    private async Task<Asset?> GetAssetForUpdateAsync(
        Guid assetId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Assets
            .FromSqlInterpolated(
                $"""SELECT * FROM "Assets" WHERE "Id" = {assetId} FOR UPDATE""")
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<AssetDto> GetRequiredAssetAsync(
        Guid assetId,
        CancellationToken cancellationToken)
    {
        return await _assetService.GetByIdAsync(
            assetId,
            cancellationToken)
            ?? throw new InvalidOperationException(
                "Asset transaction completed but the asset could not be retrieved.");
    }
}
