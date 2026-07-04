using CartApi.Application.DTO;
using CartApi.Domain;
using CartApi.Domain.ValueObjects;

namespace CartApi.Application.Services;

public static class ProductMapper
{
    public static ProductDto Map(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price.Value,
            Quantity = product.Quantity.Value,
        };
    }

    public static Product Map(ProductDto productDto)
    {
        return new Product
        {
            Id = productDto.Id,
            Name = productDto.Name,
            Price = Price.Create(productDto.Price),
            Quantity = Quantity.Create(productDto.Quantity),
        };
    }
}