using HardwareNexus.UserApi.Domain.ValueObjects;
using HardwareNexus.UserApi.Application.Services.Interfaces;
using MediatR;

namespace HardwareNexus.UserApi.Application.Features.Queries.SignIn;

public class SignInQueryHandler(IUserService userService) : IRequestHandler<SignInQuery, string?>
{
    public Task<string?> Handle(SignInQuery request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var password = Password.Create(request.Password);

        return userService.SignInAsync(email, password);
    }
}
