using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Domain.Entities;

public class AssetCalibration
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public string? ServiceProvider { get; set; }

    public string? StartNotes { get; set; }

    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }

    public string? CertificateNumber { get; set; }

    public CalibrationResult? Result { get; set; }

    public string? CompletionNotes { get; set; }

    public DateTimeOffset? NextCalibrationDueAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
