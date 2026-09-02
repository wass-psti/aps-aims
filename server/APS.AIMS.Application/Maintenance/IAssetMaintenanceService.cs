using APS.AIMS.Application.Assets;

namespace APS.AIMS.Application.Maintenance;

public interface IAssetMaintenanceService
{
    Task<IReadOnlyList<AssetMaintenanceDto>> GetHistoryAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);

    Task<AssetDto> StartAsync(
        Guid assetId,
        StartMaintenanceRequest request,
        CancellationToken cancellationToken = default);

    Task<AssetDto> CompleteAsync(
        Guid assetId,
        Guid maintenanceId,
        CompleteMaintenanceRequest request,
        CancellationToken cancellationToken = default);
}
