using MongoDB.Driver;
using ProductApi.Api.Models;
using ProductApi.Api.Repositories.Base;

namespace ProductApi.Api.Repositories;

public class GpuRepository(IMongoDatabase mongoDatabase) : MongoRepository<Gpu>(mongoDatabase)
{
    protected override string Category => ProductCategories.Gpu;
    public override Task<object> GetFiltersAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}