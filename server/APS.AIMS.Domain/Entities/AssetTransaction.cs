using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Domain.Entities;

public class AssetTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public AssetTransactionType Type { get; set; }

    public Guid? FromCustodianId { get; set; }
    public Employee? FromCustodian { get; set; }

    public Guid? ToCustodianId { get; set; }
    public Employee? ToCustodian { get; set; }

    public Guid? FromLocationId { get; set; }
    public AssetLocation? FromLocation { get; set; }

    public Guid? ToLocationId { get; set; }
    public AssetLocation? ToLocation { get; set; }

    public AssetStatus FromStatus { get; set; }
    public AssetStatus ToStatus { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
