namespace APS.AIMS.Application.Assets;

public interface IAssetService
{
    Task<IReadOnlyList<AssetDto>> GetAllAsync(
        AssetFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task<AssetDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AssetDto?> GetByAssetIdAsync(
        string assetId,
        CancellationToken cancellationToken = default);

    Task<AssetDto?> GetByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default);

    Task<AssetDto> CreateAsync(
        CreateAssetRequest request,
        CancellationToken cancellationToken = default);

    Task<AssetDto?> UpdateAsync(
        Guid id,
        UpdateAssetRequest request,
        CancellationToken cancellationToken = default);
}
