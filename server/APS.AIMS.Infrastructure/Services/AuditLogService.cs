using APS.AIMS.Application.Auditing;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly AimsDbContext _dbContext;

    public AuditLogService(AimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task WriteAsync(
        WriteAuditLogRequest request,
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            UserId = request.UserId,
            UserEmail = Clean(request.UserEmail, 250),
            UserDisplayName = Clean(request.UserDisplayName, 200),
            UserRole = Clean(request.UserRole, 50),
            Action = CleanRequired(request.Action, 150),
            Resource = CleanRequired(request.Resource, 120),
            ResourceId = Clean(request.ResourceId, 120),
            HttpMethod = CleanRequired(request.HttpMethod, 16),
            Path = CleanRequired(request.Path, 1000),
            StatusCode = request.StatusCode,
            IpAddress = Clean(request.IpAddress, 80),
            UserAgent = Clean(request.UserAgent, 500),
            OccurredAt = request.OccurredAt ?? DateTimeOffset.UtcNow
        };

        _dbContext.AuditLogs.Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetRecentAsync(
        int limit = 200,
        CancellationToken cancellationToken = default)
    {
        var safeLimit = Math.Clamp(limit, 1, 1000);

        return await _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(log => log.OccurredAt)
            .Take(safeLimit)
            .Select(log => new AuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserEmail = log.UserEmail,
                UserDisplayName = log.UserDisplayName,
                UserRole = log.UserRole,
                Action = log.Action,
                Resource = log.Resource,
                ResourceId = log.ResourceId,
                HttpMethod = log.HttpMethod,
                Path = log.Path,
                StatusCode = log.StatusCode,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                OccurredAt = log.OccurredAt
            })
            .ToListAsync(cancellationToken);
    }

    private static string CleanRequired(
        string value,
        int maxLength)
    {
        var cleaned = Clean(value, maxLength);

        return string.IsNullOrWhiteSpace(cleaned)
            ? "Unknown"
            : cleaned;
    }

    private static string? Clean(
        string? value,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var cleaned = value.Trim();

        return cleaned.Length <= maxLength
            ? cleaned
            : cleaned[..maxLength];
    }
}
