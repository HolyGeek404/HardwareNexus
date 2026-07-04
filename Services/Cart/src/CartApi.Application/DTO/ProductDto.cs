namespace CartApi.Application.DTO;

public record ProductDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required int Quantity { get; init; }
    public required int Price { get; init; }
}