using System.ComponentModel.DataAnnotations;
using MediatR;

namespace CartApi.Application.Features.Commands.AddCart;

public record AddCartCommand : IRequest<Unit>
{
    [Required] public required string UserId { get; init; }
    [Required] public required string ProductId { get; init; }
    [Required] public required string Name { get; init; }
    [Required] public required int Quantity { get; init; }
    [Required] public required int Price { get; init; }
}