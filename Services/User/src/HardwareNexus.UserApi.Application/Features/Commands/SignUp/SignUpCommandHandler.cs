using HardwareNexus.UserApi.Domain.Entities;
using HardwareNexus.UserApi.Domain.ValueObjects;
using HardwareNexus.UserApi.Application.Services.Interfaces;
using MediatR;

namespace HardwareNexus.UserApi.Application.Features.Commands.SignUp;

public class SignUpCommandHandler(IUserService userService) : IRequestHandler<SignUpCommand, bool>
{
    public Task<bool> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        var name = Name.Create(request.Name);
        var surname = Name.Create(request.Surname);
        var email = Email.Create(request.Email);
        var password = Password.Create(request.Password);
        var user = User.Create(name, surname, email, password);
        return userService.SignUpAsync(user);
    }
}