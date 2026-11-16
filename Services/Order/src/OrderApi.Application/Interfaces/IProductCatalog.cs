using OrderApi.Domain.ValueObjects;

namespace OrderApi.Application.Interfaces;

public interface IProductCatalog
{
    Task<Money?> GetUnitPriceAsync(ProductId id,  CancellationToken cancellationToken);
}