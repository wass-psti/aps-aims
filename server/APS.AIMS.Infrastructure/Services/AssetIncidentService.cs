using APS.AIMS.Application.Common;
using APS.AIMS.Application.Incidents;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Domain.Enums;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public sealed class AssetIncidentService : IAssetIncidentService
{
    private readonly AimsDbContext _dbContext;

    public AssetIncidentService(AimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AssetIncidentDto>> GetAllAsync(
        bool openOnly,
        Guid? assetId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AssetIncidents
            .AsNoTracking()
            .AsQueryable();

        if (openOnly)
        {
            query = query.Where(
                incident =>
                    incident.Status == AssetIncidentStatus.Open);
        }

        if (assetId.HasValue)
        {
            query = query.Where(
                incident => incident.AssetId == assetId.Value);
        }

        return await query
            .OrderByDescending(incident => incident.ReportedAt)
            .Select(incident => new AssetIncidentDto
            {
                Id = incident.Id,
                AssetId = incident.AssetId,
                AssetBusinessId = incident.Asset.AssetId,
                AssetName = incident.Asset.Name,
                Type = incident.Type,
                Severity = incident.Severity,
                Status = incident.Status,
                Description = incident.Description,
                OccurredAt = incident.OccurredAt,
                ReportedAt = incident.ReportedAt,
                ResolutionNotes = incident.ResolutionNotes,
                ResolvedAt = incident.ResolvedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetIncidentDto> CreateAsync(
        CreateAssetIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        var asset = await _dbContext.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == request.AssetId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Asset was not found.");

        if (asset.IsArchived)
        {
            throw new InvalidOperationException(
                "New incidents cannot be reported against an archived asset.");
        }

        var description =
            TextNormalizer.Required(request.Description);

        var occurredAt =
            request.OccurredAt ?? DateTimeOffset.UtcNow;

        if (occurredAt > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            throw new ArgumentException(
                "Incident occurrence time cannot be in the future.");
        }

        var incident = new AssetIncident
        {
            AssetId = asset.Id,
            Type = request.Type,
            Severity = request.Severity,
            Description = description,
            OccurredAt = occurredAt,
            ReportedAt = DateTimeOffset.UtcNow,
            Status = AssetIncidentStatus.Open
        };

        _dbContext.AssetIncidents.Add(incident);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(incident, asset.AssetId, asset.Name);
    }

    public async Task<AssetIncidentDto> ResolveAsync(
        Guid incidentId,
        ResolveAssetIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        var incident = await _dbContext.AssetIncidents
            .Include(item => item.Asset)
            .FirstOrDefaultAsync(
                item => item.Id == incidentId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Incident was not found.");

        if (incident.Status == AssetIncidentStatus.Resolved)
        {
            throw new InvalidOperationException(
                "The incident is already resolved.");
        }

        incident.ResolutionNotes =
            TextNormalizer.Required(request.ResolutionNotes);
        incident.ResolvedAt = DateTimeOffset.UtcNow;
        incident.Status = AssetIncidentStatus.Resolved;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(
            incident,
            incident.Asset.AssetId,
            incident.Asset.Name);
    }

    private static AssetIncidentDto ToDto(
        AssetIncident incident,
        string assetBusinessId,
        string assetName)
    {
        return new AssetIncidentDto
        {
            Id = incident.Id,
            AssetId = incident.AssetId,
            AssetBusinessId = assetBusinessId,
            AssetName = assetName,
            Type = incident.Type,
            Severity = incident.Severity,
            Status = incident.Status,
            Description = incident.Description,
            OccurredAt = incident.OccurredAt,
            ReportedAt = incident.ReportedAt,
            ResolutionNotes = incident.ResolutionNotes,
            ResolvedAt = incident.ResolvedAt
        };
    }
}
