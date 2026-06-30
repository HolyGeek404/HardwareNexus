using HardwareNexus.UserApi.Domain.ValueObjects;
using HardwareNexus.UserApi.Application.Services.Interfaces;
using MediatR;

namespace HardwareNexus.UserApi.Application.Features.Commands.AccountVerification;

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