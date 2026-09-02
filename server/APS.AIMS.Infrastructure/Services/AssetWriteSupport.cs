using APS.AIMS.Domain.Entities;
using APS.AIMS.Domain.Enums;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

internal static class AssetWriteSupport
{
    public static Task<Asset?> GetAssetForUpdateAsync(
        AimsDbContext dbContext,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        return dbContext.Assets
            .FromSqlInterpolated(
                $"""SELECT * FROM "Assets" WHERE "Id" = {assetId} FOR UPDATE""")
            .SingleOrDefaultAsync(cancellationToken);
    }

    public static AssetTransaction CreateTransaction(
        Asset asset,
        AssetTransactionType type,
        Guid? toCustodianId,
        Guid? toLocationId,
        AssetStatus toStatus,
        string? notes,
        DateTimeOffset occurredAt)
    {
        return new AssetTransaction
        {
            AssetId = asset.Id,
            Type = type,
            FromCustodianId = asset.CurrentCustodianId,
            ToCustodianId = toCustodianId,
            FromLocationId = asset.CurrentLocationId,
            ToLocationId = toLocationId,
            FromStatus = asset.Status,
            ToStatus = toStatus,
            Notes = notes,
            OccurredAt = occurredAt
        };
    }
}
