using APS.AIMS.Application.Reports;
using APS.AIMS.Domain.Enums;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public sealed class AssetReportService : IAssetReportService
{
    private readonly AimsDbContext _dbContext;

    public AssetReportService(AimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AssetReportSummaryDto> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var totalAssets = await _dbContext.Assets
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var archivedAssets = await _dbContext.Assets
            .AsNoTracking()
            .CountAsync(
                asset => asset.IsArchived,
                cancellationToken);

        var openIncidents = await _dbContext.AssetIncidents
            .AsNoTracking()
            .CountAsync(
                incident =>
                    incident.Status == AssetIncidentStatus.Open,
                cancellationToken);

        var activeCampaigns = await _dbContext.InventoryCampaigns
            .AsNoTracking()
            .CountAsync(
                campaign =>
                    campaign.Status == InventoryCampaignStatus.InProgress,
                cancellationToken);

        var statusGroups = await _dbContext.Assets
            .AsNoTracking()
            .Where(asset => !asset.IsArchived)
            .GroupBy(asset => asset.Status)
            .Select(group => new
            {
                Label = group.Key,
                Count = group.Count()
            })
            .ToListAsync(cancellationToken);

        var conditionGroups = await _dbContext.Assets
            .AsNoTracking()
            .Where(asset => !asset.IsArchived)
            .GroupBy(asset => asset.Condition)
            .Select(group => new
            {
                Label = group.Key,
                Count = group.Count()
            })
            .ToListAsync(cancellationToken);

        var branchGroups = await _dbContext.Assets
            .AsNoTracking()
            .Where(asset => !asset.IsArchived)
            .GroupBy(asset => asset.Branch.Name)
            .Select(group => new
            {
                Label = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .ToListAsync(cancellationToken);

        var categoryGroups = await _dbContext.Assets
            .AsNoTracking()
            .Where(asset => !asset.IsArchived)
            .GroupBy(asset => asset.Category.Name)
            .Select(group => new
            {
                Label = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .ToListAsync(cancellationToken);

        return new AssetReportSummaryDto
        {
            TotalAssets = totalAssets,
            ActiveAssets = totalAssets - archivedAssets,
            ArchivedAssets = archivedAssets,
            OpenIncidents = openIncidents,
            ActiveInventoryCampaigns = activeCampaigns,
            ByStatus = statusGroups
                .OrderByDescending(item => item.Count)
                .Select(item => new ReportCountDto
                {
                    Label = item.Label.ToString(),
                    Count = item.Count
                })
                .ToList(),
            ByCondition = conditionGroups
                .OrderByDescending(item => item.Count)
                .Select(item => new ReportCountDto
                {
                    Label = item.Label.ToString(),
                    Count = item.Count
                })
                .ToList(),
            ByBranch = branchGroups
                .Select(item => new ReportCountDto
                {
                    Label = item.Label,
                    Count = item.Count
                })
                .ToList(),
            ByCategory = categoryGroups
                .Select(item => new ReportCountDto
                {
                    Label = item.Label,
                    Count = item.Count
                })
                .ToList()
        };
    }
}
