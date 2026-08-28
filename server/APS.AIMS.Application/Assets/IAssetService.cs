namespace APS.AIMS.Application.Assets;

public interface IAssetService
{
    Task<IReadOnlyList<AssetDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<AssetDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AssetDto?> GetByAssetIdAsync(
        string assetId,
        CancellationToken cancellationToken = default);

    Task<AssetDto> CreateAsync(
        CreateAssetRequest request,
        CancellationToken cancellationToken = default);
}