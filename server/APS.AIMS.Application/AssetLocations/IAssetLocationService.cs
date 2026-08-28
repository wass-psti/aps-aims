namespace APS.AIMS.Application.AssetLocations;

public interface IAssetLocationService
{
    Task<IReadOnlyList<AssetLocationDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<AssetLocationDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssetLocationDto>> GetByBranchAsync(
        Guid branchId,
        CancellationToken cancellationToken = default);

    Task<AssetLocationDto> CreateAsync(
        CreateAssetLocationRequest request,
        CancellationToken cancellationToken = default);

    Task<AssetLocationDto?> UpdateAsync(
        Guid id,
        UpdateAssetLocationRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}