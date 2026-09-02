using APS.AIMS.Application.Assets;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Infrastructure.Services;

internal static class AssetLifecycleSupport
{
    public static void EnsureAvailableForService(
        Asset asset,
        string serviceName)
    {
        if (asset.IsArchived)
        {
            throw new InvalidOperationException(
                $"Archived assets cannot enter {serviceName}.");
        }

        if (asset.Status != AssetStatus.Available ||
            asset.CurrentCustodianId.HasValue)
        {
            throw new InvalidOperationException(
                $"Return the asset and make it Available before starting {serviceName}.");
        }
    }

    public static async Task<AssetDto> GetRequiredAssetAsync(
        IAssetService assetService,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        return await assetService.GetByIdAsync(
            assetId,
            cancellationToken)
            ?? throw new InvalidOperationException(
                "Asset workflow completed but the updated asset could not be retrieved.");
    }
}
