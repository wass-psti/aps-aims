namespace APS.AIMS.Application.Reports;

public sealed class AssetReportSummaryDto
{
    public int TotalAssets { get; init; }

    public int ActiveAssets { get; init; }

    public int ArchivedAssets { get; init; }

    public int OpenIncidents { get; init; }

    public int ActiveInventoryCampaigns { get; init; }

    public required IReadOnlyList<ReportCountDto> ByStatus { get; init; }

    public required IReadOnlyList<ReportCountDto> ByCondition { get; init; }

    public required IReadOnlyList<ReportCountDto> ByBranch { get; init; }

    public required IReadOnlyList<ReportCountDto> ByCategory { get; init; }
}
