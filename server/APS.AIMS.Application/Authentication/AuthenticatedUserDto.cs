namespace APS.AIMS.Application.Authentication;

public sealed class AuthenticatedUserDto
{
    public Guid Id { get; init; }

    public required string Email { get; init; }

    public required string DisplayName { get; init; }

    public required string Role { get; init; }
}
