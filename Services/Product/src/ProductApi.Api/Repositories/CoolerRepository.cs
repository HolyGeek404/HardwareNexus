using MongoDB.Driver;
using ProductApi.Api.Models;
using ProductApi.Api.Repositories.Base;

namespace ProductApi.Api.Repositories;

public class CoolerRepository(IMongoDatabase mongoDatabase) : MongoRepository<Cooler>(mongoDatabase)
{
    protected override string Category => ProductCategories.Cooler;
    public override Task<object> GetFiltersAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}