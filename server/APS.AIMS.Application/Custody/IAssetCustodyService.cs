using APS.AIMS.Application.Assets;

namespace APS.AIMS.Application.Custody;

public interface IAssetCustodyService
{
    Task<IReadOnlyList<AssetCustodyHistoryDto>> GetHistoryAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);

    Task<AssetDto> IssueAsync(
        Guid assetId,
        IssueAssetRequest request,
        CancellationToken cancellationToken = default);

    Task<AssetDto> ReturnAsync(
        Guid assetId,
        ReturnAssetRequest request,
        CancellationToken cancellationToken = default);
}
