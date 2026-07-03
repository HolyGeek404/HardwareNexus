using System.ComponentModel.DataAnnotations;
using MediatR;

namespace UserApi.Application.Features.Commands.Delete;

public class DeleteCommand : IRequest
{
    [Required] [EmailAddress] public required string Email { get; set; }
}