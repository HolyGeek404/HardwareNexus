namespace OrderApi.Application.DTO;

public record OrderLineDto
{
    public required Guid ProductId { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal TotalPrice { get; init; }
    public required int Quantity { get; init; }
}