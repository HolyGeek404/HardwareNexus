using UserApi.Domain.ValueObjects;
using MediatR;
using UserApi.Application.Services.Interfaces;

namespace UserApi.Application.Features.Commands.AccountVerification;

public class AccountVerificationCommandHandler(IUserService userService)
    : IRequestHandler<AccountVerificationCommand, bool>
{
    public async Task<bool> Handle(AccountVerificationCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var token = ActivationToken.Create(request.VerificationKey);

        var result = await userService.ActivateUserAsync(email, token);
        return result;
    }
}