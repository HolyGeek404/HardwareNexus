using MediatR;
using UserApi.Application.Services.Interfaces;
using UserApi.Domain.ValueObjects;

namespace UserApi.Application.Features.Commands.Delete;

public class DeleteCommandHandler(IUserService userService) : IRequestHandler<DeleteCommand>
{
    public Task Handle(DeleteCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        userService.RemoveUserAsync(email);
        return Task.CompletedTask;
    }
}