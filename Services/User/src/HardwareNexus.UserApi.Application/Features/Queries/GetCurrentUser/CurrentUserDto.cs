namespace HardwareNexus.UserApi.Application.Features.Queries.GetCurrentUser;

public sealed record CurrentUserDto
{
    public required string Name { get; init; }
    public required string Surname { get; init; }
    public required string Email { get; init; }
}
