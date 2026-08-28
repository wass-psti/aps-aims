namespace APS.AIMS.Application.Custody;

public sealed class IssueAssetRequest
{
    public Guid EmployeeId { get; init; }

    public string? Notes { get; init; }
}
