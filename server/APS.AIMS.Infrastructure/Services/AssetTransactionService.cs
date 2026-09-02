using APS.AIMS.Application.Assets;
using APS.AIMS.Application.Common;
using APS.AIMS.Application.Transactions;
using APS.AIMS.Domain.Enums;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public sealed class AssetTransactionService : IAssetTransactionService
{
    private readonly AimsDbContext _dbContext;
    private readonly IAssetService _assetService;

    public AssetTransactionService(
        AimsDbContext dbContext,
        IAssetService assetService)
    {
        _dbContext = dbContext;
        _assetService = assetService;
    }

    public async Task<IReadOnlyList<AssetTransactionDto>> GetHistoryAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssetTransactions
            .AsNoTracking()
            .Where(transaction => transaction.AssetId == assetId)
            .OrderByDescending(transaction => transaction.OccurredAt)
            .Select(transaction => new AssetTransactionDto
            {
                Id = transaction.Id,
                AssetId = transaction.AssetId,
                Type = transaction.Type,

                FromCustodianId = transaction.FromCustodianId,
                FromCustodianName = transaction.FromCustodian != null
                    ? transaction.FromCustodian.FirstName + " " +
                      transaction.FromCustodian.LastName
                    : null,

                ToCustodianId = transaction.ToCustodianId,
                ToCustodianName = transaction.ToCustodian != null
                    ? transaction.ToCustodian.FirstName + " " +
                      transaction.ToCustodian.LastName
                    : null,

                FromLocationId = transaction.FromLocationId,
                FromLocationName = transaction.FromLocation != null
                    ? transaction.FromLocation.Name
                    : null,

                ToLocationId = transaction.ToLocationId,
                ToLocationName = transaction.ToLocation != null
                    ? transaction.ToLocation.Name
                    : null,

                FromStatus = transaction.FromStatus,
                ToStatus = transaction.ToStatus,
                Notes = transaction.Notes,
                OccurredAt = transaction.OccurredAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetDto> TransferAsync(
        Guid assetId,
        TransferAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var asset = await AssetWriteSupport.GetAssetForUpdateAsync(
            _dbContext,
            assetId,
            cancellationToken);

        if (asset is null)
        {
            throw new KeyNotFoundException("Asset was not found.");
        }

        if (asset.IsArchived)
        {
            throw new InvalidOperationException(
                "Archived assets cannot be transferred.");
        }

        if (asset.Status != AssetStatus.Available ||
            asset.CurrentCustodianId.HasValue)
        {
            throw new InvalidOperationException(
                "Return the asset before transferring it to another location.");
        }

        var destination = await _dbContext.AssetLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                location =>
                    location.Id == request.LocationId &&
                    location.BranchId == asset.BranchId &&
                    location.IsActive,
                cancellationToken);

        if (destination is null)
        {
            throw new ArgumentException(
                "Selected transfer location is invalid for this asset's branch.");
        }

        if (destination.Id == asset.CurrentLocationId)
        {
            throw new InvalidOperationException(
                "The selected destination is already the asset's current location.");
        }

        var now = DateTimeOffset.UtcNow;
        var notes = TextNormalizer.Optional(request.Notes);

        var transactionRecord = AssetWriteSupport.CreateTransaction(
            asset,
            AssetTransactionType.Transfer,
            null,
            destination.Id,
            AssetStatus.Available,
            notes,
            now);

        asset.CurrentLocationId = destination.Id;
        asset.UpdatedAt = now;

        _dbContext.AssetTransactions.Add(transactionRecord);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await _assetService.GetByIdAsync(
            asset.Id,
            cancellationToken)
            ?? throw new InvalidOperationException(
                "Asset transfer completed but the asset could not be retrieved.");
    }
}
