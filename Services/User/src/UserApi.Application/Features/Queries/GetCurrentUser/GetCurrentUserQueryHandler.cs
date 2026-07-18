using MediatR;
using UserApi.Application.Services.Interfaces;
using UserApi.Domain.ValueObjects;

namespace UserApi.Application.Features.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(IUserService userService)
    : IRequestHandler<GetCurrentUserQuery, CurrentUserDto?>
{
    public async Task<CurrentUserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var user = await userService.GetUserByEmailAsync(email);
        if (user == null) return null;

        return new CurrentUserDto
        {
            Name = user.Name.Value,
            Surname = user.Surname.Value,
            Email = user.Email.Value
        };
    }
}