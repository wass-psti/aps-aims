namespace APS.AIMS.Application.Reports;

public interface IAssetReportService
{
    Task<AssetReportSummaryDto> GetSummaryAsync(
        CancellationToken cancellationToken = default);
}
