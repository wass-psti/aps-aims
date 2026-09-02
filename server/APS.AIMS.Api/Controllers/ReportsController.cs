using APS.AIMS.Application.Reports;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(
    IAssetReportService reportService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<AssetReportSummaryDto>> GetSummary(
        CancellationToken cancellationToken)
    {
        return Ok(
            await reportService.GetSummaryAsync(
                cancellationToken));
    }
}
