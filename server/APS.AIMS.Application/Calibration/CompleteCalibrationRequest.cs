using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Application.Calibration;

public sealed class CompleteCalibrationRequest
{
    public string? CertificateNumber { get; init; }

    public CalibrationResult Result { get; init; }

    public string? CompletionNotes { get; init; }

    public DateTimeOffset? NextCalibrationDueAt { get; init; }
}
