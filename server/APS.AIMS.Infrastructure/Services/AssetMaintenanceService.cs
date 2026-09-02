using APS.AIMS.Application.Assets;
using APS.AIMS.Application.Common;
using APS.AIMS.Application.Maintenance;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Domain.Enums;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public sealed class AssetMaintenanceService : IAssetMaintenanceService
{
    private readonly AimsDbContext _dbContext;
    private readonly IAssetService _assetService;

    public AssetMaintenanceService(
        AimsDbContext dbContext,
        IAssetService assetService)
    {
        _dbContext = dbContext;
        _assetService = assetService;
    }

    public async Task<IReadOnlyList<AssetMaintenanceDto>> GetHistoryAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssetMaintenances
            .AsNoTracking()
            .Where(record => record.AssetId == assetId)
            .OrderByDescending(record => record.StartedAt)
            .Select(record => new AssetMaintenanceDto
            {
                Id = record.Id,
                AssetId = record.AssetId,
                Description = record.Description,
                ServiceProvider = record.ServiceProvider,
                StartNotes = record.StartNotes,
                StartedAt = record.StartedAt,
                CompletedAt = record.CompletedAt,
                CompletionNotes = record.CompletionNotes,
                Cost = record.Cost,
                Currency = record.Currency,
                NextMaintenanceDueAt = record.NextMaintenanceDueAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetDto> StartAsync(
        Guid assetId,
        StartMaintenanceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ArgumentException(
                "Maintenance description is required.");
        }

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var asset = await AssetWriteSupport.GetAssetForUpdateAsync(
            _dbContext,
            assetId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Asset was not found.");

        AssetLifecycleSupport.EnsureAvailableForService(
            asset,
            "maintenance");

        var conflictingRecordExists =
            await _dbContext.AssetCalibrations
                .AsNoTracking()
                .AnyAsync(
                    record =>
                        record.AssetId == asset.Id &&
                        record.CompletedAt == null,
                    cancellationToken);

        if (conflictingRecordExists)
        {
            throw new InvalidOperationException(
                "Complete the open calibration record before starting maintenance.");
        }

        var now = DateTimeOffset.UtcNow;
        var notes = TextNormalizer.Optional(request.Notes);

        var record = new AssetMaintenance
        {
            AssetId = asset.Id,
            Description = TextNormalizer.Required(request.Description),
            ServiceProvider = TextNormalizer.Optional(request.ServiceProvider),
            StartNotes = notes,
            StartedAt = now
        };

        var transactionRecord = AssetWriteSupport.CreateTransaction(
            asset,
            AssetTransactionType.MaintenanceStart,
            asset.CurrentCustodianId,
            asset.CurrentLocationId,
            AssetStatus.UnderMaintenance,
            notes ?? record.Description,
            now);

        asset.Status = AssetStatus.UnderMaintenance;
        asset.UpdatedAt = now;

        _dbContext.AssetMaintenances.Add(record);
        _dbContext.AssetTransactions.Add(transactionRecord);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await AssetLifecycleSupport.GetRequiredAssetAsync(
            _assetService,
            asset.Id,
            cancellationToken);
    }

    public async Task<AssetDto> CompleteAsync(
        Guid assetId,
        Guid maintenanceId,
        CompleteMaintenanceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Cost < 0)
        {
            throw new ArgumentException(
                "Maintenance cost cannot be negative.");
        }

        var normalizedCurrency =
            TextNormalizer.CodeOrNull(request.Currency);

        if (normalizedCurrency is not null &&
            normalizedCurrency.Length != 3)
        {
            throw new ArgumentException(
                "Currency must use a three-letter ISO code.");
        }

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var asset = await AssetWriteSupport.GetAssetForUpdateAsync(
            _dbContext,
            assetId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Asset was not found.");

        if (asset.Status != AssetStatus.UnderMaintenance)
        {
            throw new InvalidOperationException(
                "The asset is not currently under maintenance.");
        }

        var record = await _dbContext.AssetMaintenances
            .FirstOrDefaultAsync(
                item =>
                    item.Id == maintenanceId &&
                    item.AssetId == asset.Id &&
                    item.CompletedAt == null,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Open maintenance record was not found.");

        var now = DateTimeOffset.UtcNow;
        var completionNotes =
            TextNormalizer.Optional(request.CompletionNotes);

        var transactionRecord = AssetWriteSupport.CreateTransaction(
            asset,
            AssetTransactionType.MaintenanceComplete,
            asset.CurrentCustodianId,
            asset.CurrentLocationId,
            AssetStatus.Available,
            completionNotes,
            now);

        record.CompletedAt = now;
        record.CompletionNotes = completionNotes;
        record.Cost = request.Cost;
        record.Currency = normalizedCurrency;
        record.NextMaintenanceDueAt =
            request.NextMaintenanceDueAt;

        asset.Status = AssetStatus.Available;
        asset.UpdatedAt = now;

        _dbContext.AssetTransactions.Add(transactionRecord);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await AssetLifecycleSupport.GetRequiredAssetAsync(
            _assetService,
            asset.Id,
            cancellationToken);
    }
}
