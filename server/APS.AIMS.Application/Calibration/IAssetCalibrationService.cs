using APS.AIMS.Application.Assets;

namespace APS.AIMS.Application.Calibration;

public interface IAssetCalibrationService
{
    Task<IReadOnlyList<AssetCalibrationDto>> GetHistoryAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);

    Task<AssetDto> StartAsync(
        Guid assetId,
        StartCalibrationRequest request,
        CancellationToken cancellationToken = default);

    Task<AssetDto> CompleteAsync(
        Guid assetId,
        Guid calibrationId,
        CompleteCalibrationRequest request,
        CancellationToken cancellationToken = default);
}
