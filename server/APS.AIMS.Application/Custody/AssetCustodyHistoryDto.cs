namespace APS.AIMS.Application.Custody;

public sealed class AssetCustodyHistoryDto
{
    public Guid Id { get; init; }

    public Guid AssetId { get; init; }

    public Guid EmployeeId { get; init; }

    public required string EmployeeName { get; init; }

    public string? EmployeeNumber { get; init; }

    public Guid IssuedFromLocationId { get; init; }

    public required string IssuedFromLocationName { get; init; }

    public Guid? ReturnedToLocationId { get; init; }

    public string? ReturnedToLocationName { get; init; }

    public DateTimeOffset IssuedAt { get; init; }

    public DateTimeOffset? ReturnedAt { get; init; }

    public string? IssueNotes { get; init; }

    public string? ReturnNotes { get; init; }

    public bool IsOpen => ReturnedAt is null;
}
