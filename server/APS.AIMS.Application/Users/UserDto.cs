namespace APS.AIMS.Application.Users;

public sealed class UserDto
{
    public Guid Id { get; init; }

    public required string Email { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string DisplayName { get; init; }

    public required string Role { get; init; }

    public bool IsActive { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? LastLoginAt { get; init; }
}
