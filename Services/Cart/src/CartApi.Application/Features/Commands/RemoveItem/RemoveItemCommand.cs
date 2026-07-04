using System.ComponentModel.DataAnnotations;
using MediatR;

namespace CartApi.Application.Features.Commands.RemoveItem;

public class RemoveItemCommand : IRequest<Unit>
{
    [Required] public required string UserId { get; init; }
    [Required] public required string ProductId { get; init; }
}
