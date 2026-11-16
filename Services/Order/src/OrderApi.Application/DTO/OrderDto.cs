namespace OrderApi.Application.DTO;

public record OrderDto
{
    public required Guid OrderId { get; init; }
    public required Guid CustomerId { get; init; }
    public required decimal Total { get; init; }
    public required string Currency { get; init; }
    public required string Status { get; init; }
    public required OrderLineDto[] OrderLines { get; init; }  = [];
    public required DateTime OrderDate { get; init; }
}