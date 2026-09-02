namespace APS.AIMS.Application.Users;

public sealed class CreateUserRequest
{
    public required string Email { get; init; }

    public required string Password { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Role { get; init; }
}
