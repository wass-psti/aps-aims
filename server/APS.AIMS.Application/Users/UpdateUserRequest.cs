namespace APS.AIMS.Application.Users;

public sealed class UpdateUserRequest
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Role { get; init; }

    public bool IsActive { get; init; }
}
