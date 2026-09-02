namespace APS.AIMS.Application.Calibration;

public sealed class StartCalibrationRequest
{
    public string? ServiceProvider { get; init; }

    public string? Notes { get; init; }
}
