using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Application.Transactions;

public sealed class AssetTransactionDto
{
    public Guid Id { get; init; }

    public Guid AssetId { get; init; }

    public AssetTransactionType Type { get; init; }

    public Guid? FromCustodianId { get; init; }

    public string? FromCustodianName { get; init; }

    public Guid? ToCustodianId { get; init; }

    public string? ToCustodianName { get; init; }

    public Guid? FromLocationId { get; init; }

    public string? FromLocationName { get; init; }

    public Guid? ToLocationId { get; init; }

    public string? ToLocationName { get; init; }

    public AssetStatus FromStatus { get; init; }

    public AssetStatus ToStatus { get; init; }

    public string? Notes { get; init; }

    public DateTimeOffset OccurredAt { get; init; }
}
