using ProductApi.Api.Interfaces;
using ProductApi.Api.Models;

namespace ProductApi.Api.Factories;

public class ProductRepositoryFactory(IServiceProvider  serviceProvider) : IProductRepositoryFactory
{
    public IProductRepository Create(ProductCategory category)
    {
        return serviceProvider.GetRequiredKeyedService<IProductRepository>(category);
    }
}