namespace APS.AIMS.Application.Authentication;

public sealed class LoginResponse
{
    public required string AccessToken { get; init; }

    public required AuthenticatedUserDto User { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }
}
