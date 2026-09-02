namespace APS.AIMS.Application.Auditing;

public sealed class WriteAuditLogRequest
{
    public Guid? UserId { get; init; }

    public string? UserEmail { get; init; }

    public string? UserDisplayName { get; init; }

    public string? UserRole { get; init; }

    public required string Action { get; init; }

    public required string Resource { get; init; }

    public string? ResourceId { get; init; }

    public required string HttpMethod { get; init; }

    public required string Path { get; init; }

    public int StatusCode { get; init; }

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }

    public DateTimeOffset? OccurredAt { get; init; }
}
