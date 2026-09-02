using APS.AIMS.Application.Assets;

namespace APS.AIMS.Application.Transactions;

public interface IAssetTransactionService
{
    Task<IReadOnlyList<AssetTransactionDto>> GetHistoryAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);

    Task<AssetDto> TransferAsync(
        Guid assetId,
        TransferAssetRequest request,
        CancellationToken cancellationToken = default);
}
