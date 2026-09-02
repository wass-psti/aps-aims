namespace APS.AIMS.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserId { get; set; }

    public string? UserEmail { get; set; }

    public string? UserDisplayName { get; set; }

    public string? UserRole { get; set; }

    public required string Action { get; set; }

    public required string Resource { get; set; }

    public string? ResourceId { get; set; }

    public required string HttpMethod { get; set; }

    public required string Path { get; set; }

    public int StatusCode { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTimeOffset OccurredAt { get; set; } =
        DateTimeOffset.UtcNow;
}
