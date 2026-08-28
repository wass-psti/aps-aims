namespace APS.AIMS.Domain.Entities;

public class AssetCustodyHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid IssuedFromLocationId { get; set; }
    public AssetLocation IssuedFromLocation { get; set; } = null!;

    public Guid? ReturnedToLocationId { get; set; }
    public AssetLocation? ReturnedToLocation { get; set; }

    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReturnedAt { get; set; }

    public string? IssueNotes { get; set; }
    public string? ReturnNotes { get; set; }
}
