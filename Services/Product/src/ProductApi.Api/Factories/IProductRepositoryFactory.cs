using ProductApi.Api.Interfaces;
using ProductApi.Api.Models;

namespace ProductApi.Api.Factories;

public interface IProductRepositoryFactory
{
    IProductRepository Create(ProductCategory category);
}