namespace APS.AIMS.Application.Auditing;

public interface IAuditLogService
{
    Task WriteAsync(
        WriteAuditLogRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDto>> GetRecentAsync(
        int limit = 200,
        CancellationToken cancellationToken = default);
}
