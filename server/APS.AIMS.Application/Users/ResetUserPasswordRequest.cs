namespace APS.AIMS.Application.Users;

public sealed class ResetUserPasswordRequest
{
    public required string NewPassword { get; init; }
}
