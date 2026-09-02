using APS.AIMS.Application.Assets;
using APS.AIMS.Application.Transactions;
using APS.AIMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/assets/{assetId:guid}/transactions")]
public sealed class AssetTransactionsController(
    IAssetTransactionService transactionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetTransactionDto>>> GetHistory(
        Guid assetId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await transactionService.GetHistoryAsync(
                assetId,
                cancellationToken));
    }

    [Authorize(Roles = AimsAuthorization.CanTransferAssets)]
    [HttpPost("transfer")]
    public async Task<ActionResult<AssetDto>> Transfer(
        Guid assetId,
        TransferAssetRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await transactionService.TransferAsync(
                assetId,
                request,
                cancellationToken));
    }
}
