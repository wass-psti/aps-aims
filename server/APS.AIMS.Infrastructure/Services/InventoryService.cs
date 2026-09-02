using APS.AIMS.Application.Common;
using APS.AIMS.Application.Inventory;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Domain.Enums;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly AimsDbContext _dbContext;

    public InventoryService(AimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InventoryCampaignDto>> GetCampaignsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.InventoryCampaigns
            .AsNoTracking()
            .OrderByDescending(campaign => campaign.CreatedAt)
            .Select(campaign => new InventoryCampaignDto
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Description = campaign.Description,
                BranchId = campaign.BranchId,
                BranchName = campaign.Branch.Name,
                Status = campaign.Status,
                CountedAssets = _dbContext.InventoryCounts.Count(
                    count => count.CampaignId == campaign.Id),
                CreatedAt = campaign.CreatedAt,
                StartedAt = campaign.StartedAt,
                CompletedAt = campaign.CompletedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryCampaignDto> CreateCampaignAsync(
        CreateInventoryCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = TextNormalizer.Required(request.Name);

        var branch = await _dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == request.BranchId && item.IsActive,
                cancellationToken)
            ?? throw new ArgumentException(
                "Selected branch is invalid or inactive.");

        var campaign = new InventoryCampaign
        {
            Name = name,
            Description = TextNormalizer.Optional(request.Description),
            BranchId = branch.Id,
            Status = InventoryCampaignStatus.Draft
        };

        _dbContext.InventoryCampaigns.Add(campaign);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(campaign, branch.Name, 0);
    }

    public async Task<InventoryCampaignDto> StartCampaignAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await _dbContext.InventoryCampaigns
            .Include(item => item.Branch)
            .FirstOrDefaultAsync(
                item => item.Id == campaignId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Inventory campaign was not found.");

        if (campaign.Status != InventoryCampaignStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only Draft inventory campaigns can be started.");
        }

        campaign.Status = InventoryCampaignStatus.InProgress;
        campaign.StartedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(
            campaign,
            campaign.Branch.Name,
            await CountCampaignAssetsAsync(campaign.Id, cancellationToken));
    }

    public async Task<InventoryCampaignDto> CompleteCampaignAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await _dbContext.InventoryCampaigns
            .Include(item => item.Branch)
            .FirstOrDefaultAsync(
                item => item.Id == campaignId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Inventory campaign was not found.");

        if (campaign.Status != InventoryCampaignStatus.InProgress)
        {
            throw new InvalidOperationException(
                "Only an In Progress inventory campaign can be completed.");
        }

        campaign.Status = InventoryCampaignStatus.Completed;
        campaign.CompletedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(
            campaign,
            campaign.Branch.Name,
            await CountCampaignAssetsAsync(campaign.Id, cancellationToken));
    }

    public async Task<IReadOnlyList<InventoryCountDto>> GetCountsAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var campaignExists = await _dbContext.InventoryCampaigns
            .AsNoTracking()
            .AnyAsync(
                campaign => campaign.Id == campaignId,
                cancellationToken);

        if (!campaignExists)
        {
            throw new KeyNotFoundException(
                "Inventory campaign was not found.");
        }

        return await _dbContext.InventoryCounts
            .AsNoTracking()
            .Where(count => count.CampaignId == campaignId)
            .OrderByDescending(count => count.CountedAt)
            .Select(count => new InventoryCountDto
            {
                Id = count.Id,
                CampaignId = count.CampaignId,
                AssetId = count.AssetId,
                AssetBusinessId = count.Asset.AssetId,
                AssetName = count.Asset.Name,
                BarcodeValue = count.Asset.BarcodeValue,
                SystemLocationId = count.SystemLocationId,
                SystemLocationName = count.SystemLocation.Name,
                ObservedLocationId = count.ObservedLocationId,
                ObservedLocationName = count.ObservedLocation.Name,
                SystemCondition = count.SystemCondition,
                ObservedCondition = count.ObservedCondition,
                Result = count.Result,
                Notes = count.Notes,
                CountedAt = count.CountedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryCountDto> RecordCountAsync(
        Guid campaignId,
        RecordInventoryCountRequest request,
        CancellationToken cancellationToken = default)
    {
        var campaign = await _dbContext.InventoryCampaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == campaignId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Inventory campaign was not found.");

        if (campaign.Status != InventoryCampaignStatus.InProgress)
        {
            throw new InvalidOperationException(
                "Asset counts can only be recorded while the campaign is In Progress.");
        }

        var barcode = TextNormalizer.Required(request.BarcodeValue);

        var asset = await _dbContext.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.BarcodeValue == barcode,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                $"No asset was found for barcode '{barcode}'.");

        if (asset.IsArchived)
        {
            throw new InvalidOperationException(
                "Archived assets cannot be recorded in an active inventory campaign.");
        }

        if (asset.BranchId != campaign.BranchId)
        {
            throw new InvalidOperationException(
                "The scanned asset belongs to a different branch than this inventory campaign.");
        }

        var observedLocation = await _dbContext.AssetLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                location =>
                    location.Id == request.ObservedLocationId &&
                    location.BranchId == campaign.BranchId &&
                    location.IsActive,
                cancellationToken)
            ?? throw new ArgumentException(
                "Observed location is invalid for this inventory campaign.");

        var observedCondition =
            request.ObservedCondition ?? asset.Condition;

        var result = GetResult(
            asset.CurrentLocationId == observedLocation.Id,
            asset.Condition == observedCondition);

        var now = DateTimeOffset.UtcNow;
        var notes = TextNormalizer.Optional(request.Notes);

        var count = await _dbContext.InventoryCounts
            .FirstOrDefaultAsync(
                item =>
                    item.CampaignId == campaign.Id &&
                    item.AssetId == asset.Id,
                cancellationToken);

        if (count is null)
        {
            count = new InventoryCount
            {
                CampaignId = campaign.Id,
                AssetId = asset.Id,
                SystemLocationId = asset.CurrentLocationId,
                ObservedLocationId = observedLocation.Id,
                SystemCondition = asset.Condition,
                ObservedCondition = observedCondition,
                Result = result,
                Notes = notes,
                CountedAt = now
            };

            _dbContext.InventoryCounts.Add(count);
        }
        else
        {
            count.SystemLocationId = asset.CurrentLocationId;
            count.ObservedLocationId = observedLocation.Id;
            count.SystemCondition = asset.Condition;
            count.ObservedCondition = observedCondition;
            count.Result = result;
            count.Notes = notes;
            count.CountedAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new InventoryCountDto
        {
            Id = count.Id,
            CampaignId = campaign.Id,
            AssetId = asset.Id,
            AssetBusinessId = asset.AssetId,
            AssetName = asset.Name,
            BarcodeValue = asset.BarcodeValue,
            SystemLocationId = asset.CurrentLocationId,
            SystemLocationName =
                await GetLocationNameAsync(
                    asset.CurrentLocationId,
                    cancellationToken),
            ObservedLocationId = observedLocation.Id,
            ObservedLocationName = observedLocation.Name,
            SystemCondition = asset.Condition,
            ObservedCondition = observedCondition,
            Result = result,
            Notes = notes,
            CountedAt = now
        };
    }

    private Task<int> CountCampaignAssetsAsync(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        return _dbContext.InventoryCounts
            .CountAsync(
                count => count.CampaignId == campaignId,
                cancellationToken);
    }

    private async Task<string> GetLocationNameAsync(
        Guid locationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.AssetLocations
            .AsNoTracking()
            .Where(location => location.Id == locationId)
            .Select(location => location.Name)
            .SingleAsync(cancellationToken);
    }

    private static InventoryCountResult GetResult(
        bool locationMatches,
        bool conditionMatches)
    {
        if (locationMatches && conditionMatches)
        {
            return InventoryCountResult.Matched;
        }

        if (!locationMatches && !conditionMatches)
        {
            return InventoryCountResult.LocationAndConditionMismatch;
        }

        return locationMatches
            ? InventoryCountResult.ConditionMismatch
            : InventoryCountResult.LocationMismatch;
    }

    private static InventoryCampaignDto ToDto(
        InventoryCampaign campaign,
        string branchName,
        int countedAssets)
    {
        return new InventoryCampaignDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            BranchId = campaign.BranchId,
            BranchName = branchName,
            Status = campaign.Status,
            CountedAssets = countedAssets,
            CreatedAt = campaign.CreatedAt,
            StartedAt = campaign.StartedAt,
            CompletedAt = campaign.CompletedAt
        };
    }
}
