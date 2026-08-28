namespace APS.AIMS.Application.AssetCategories;

public interface IAssetCategoryService
{
    Task<IReadOnlyList<AssetCategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<AssetCategoryDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AssetCategoryDto> CreateAsync(
        CreateAssetCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<AssetCategoryDto?> UpdateAsync(
        Guid id,
        UpdateAssetCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}