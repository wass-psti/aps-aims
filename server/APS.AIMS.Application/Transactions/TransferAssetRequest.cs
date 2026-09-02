namespace APS.AIMS.Application.Transactions;

public sealed class TransferAssetRequest
{
    public Guid LocationId { get; init; }

    public string? Notes { get; init; }
}
