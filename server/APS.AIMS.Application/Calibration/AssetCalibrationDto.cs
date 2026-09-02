using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Application.Calibration;

public sealed class AssetCalibrationDto
{
    public Guid Id { get; init; }

    public Guid AssetId { get; init; }

    public string? ServiceProvider { get; init; }

    public string? StartNotes { get; init; }

    public DateTimeOffset StartedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }

    public string? CertificateNumber { get; init; }

    public CalibrationResult? Result { get; init; }

    public string? CompletionNotes { get; init; }

    public DateTimeOffset? NextCalibrationDueAt { get; init; }

    public bool IsOpen => CompletedAt is null;
}
