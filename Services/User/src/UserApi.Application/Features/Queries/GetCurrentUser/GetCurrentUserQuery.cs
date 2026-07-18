using MediatR;

namespace UserApi.Application.Features.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<CurrentUserDto?>
{
    public required string Email { get; init; }
}