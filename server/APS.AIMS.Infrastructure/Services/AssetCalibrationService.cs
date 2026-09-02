using APS.AIMS.Application.Assets;
using APS.AIMS.Application.Calibration;
using APS.AIMS.Application.Common;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Domain.Enums;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public sealed class AssetCalibrationService : IAssetCalibrationService
{
    private readonly AimsDbContext _dbContext;
    private readonly IAssetService _assetService;

    public AssetCalibrationService(
        AimsDbContext dbContext,
        IAssetService assetService)
    {
        _dbContext = dbContext;
        _assetService = assetService;
    }

    public async Task<IReadOnlyList<AssetCalibrationDto>> GetHistoryAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssetCalibrations
            .AsNoTracking()
            .Where(record => record.AssetId == assetId)
            .OrderByDescending(record => record.StartedAt)
            .Select(record => new AssetCalibrationDto
            {
                Id = record.Id,
                AssetId = record.AssetId,
                ServiceProvider = record.ServiceProvider,
                StartNotes = record.StartNotes,
                StartedAt = record.StartedAt,
                CompletedAt = record.CompletedAt,
                CertificateNumber = record.CertificateNumber,
                Result = record.Result,
                CompletionNotes = record.CompletionNotes,
                NextCalibrationDueAt = record.NextCalibrationDueAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetDto> StartAsync(
        Guid assetId,
        StartCalibrationRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var asset = await AssetWriteSupport.GetAssetForUpdateAsync(
            _dbContext,
            assetId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Asset was not found.");

        AssetLifecycleSupport.EnsureAvailableForService(
            asset,
            "calibration");

        var conflictingRecordExists =
            await _dbContext.AssetMaintenances
                .AsNoTracking()
                .AnyAsync(
                    record =>
                        record.AssetId == asset.Id &&
                        record.CompletedAt == null,
                    cancellationToken);

        if (conflictingRecordExists)
        {
            throw new InvalidOperationException(
                "Complete the open maintenance record before starting calibration.");
        }

        var now = DateTimeOffset.UtcNow;
        var notes = TextNormalizer.Optional(request.Notes);

        var record = new AssetCalibration
        {
            AssetId = asset.Id,
            ServiceProvider = TextNormalizer.Optional(request.ServiceProvider),
            StartNotes = notes,
            StartedAt = now
        };

        var transactionRecord = AssetWriteSupport.CreateTransaction(
            asset,
            AssetTransactionType.CalibrationStart,
            asset.CurrentCustodianId,
            asset.CurrentLocationId,
            AssetStatus.UnderCalibration,
            notes,
            now);

        asset.Status = AssetStatus.UnderCalibration;
        asset.UpdatedAt = now;

        _dbContext.AssetCalibrations.Add(record);
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
        Guid calibrationId,
        CompleteCalibrationRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var asset = await AssetWriteSupport.GetAssetForUpdateAsync(
            _dbContext,
            assetId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Asset was not found.");

        if (asset.Status != AssetStatus.UnderCalibration)
        {
            throw new InvalidOperationException(
                "The asset is not currently under calibration.");
        }

        var record = await _dbContext.AssetCalibrations
            .FirstOrDefaultAsync(
                item =>
                    item.Id == calibrationId &&
                    item.AssetId == asset.Id &&
                    item.CompletedAt == null,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Open calibration record was not found.");

        var now = DateTimeOffset.UtcNow;
        var completionNotes =
            TextNormalizer.Optional(request.CompletionNotes);

        var transactionRecord = AssetWriteSupport.CreateTransaction(
            asset,
            AssetTransactionType.CalibrationComplete,
            asset.CurrentCustodianId,
            asset.CurrentLocationId,
            AssetStatus.Available,
            completionNotes,
            now);

        record.CompletedAt = now;
        record.CertificateNumber =
            TextNormalizer.Optional(request.CertificateNumber);
        record.Result = request.Result;
        record.CompletionNotes = completionNotes;
        record.NextCalibrationDueAt =
            request.NextCalibrationDueAt;

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
