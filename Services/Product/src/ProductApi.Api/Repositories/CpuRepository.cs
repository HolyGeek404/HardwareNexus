using MongoDB.Driver;
using ProductApi.Api.Interfaces;
using ProductApi.Api.Models;
using ProductApi.Api.Repositories.Base;

namespace ProductApi.Api.Repositories;

public class CpuRepository(IMongoDatabase mongoDatabase) : MongoRepository<Cpu>(mongoDatabase), ICpuRepository
{
    public async Task<CpuFilters> GetFiltersAsync(string category)
    {
        var categoryFilter = Builders<Cpu>.Filter.Eq(c => c.Category, category);

        return new CpuFilters
        {
            Team = await GetDistinctValuesAsync(c => c.Team, categoryFilter),
            Cores = await GetDistinctValuesAsync(c => c.Cores, categoryFilter),
            Socket = await GetDistinctValuesAsync(c => c.Socket, categoryFilter),
            Architecture = await GetDistinctValuesAsync(c => c.Architecture, categoryFilter),
            TDP = await GetDistinctValuesAsync(c => c.Tdp, categoryFilter)
        };
    }

    private async Task<string[]> GetDistinctValuesAsync(
        System.Linq.Expressions.Expression<Func<Cpu, string>> field,
        FilterDefinition<Cpu> filter)
    {
        var results = await Collection.DistinctAsync(field, filter);
        var values = new List<string>();
        while (await results.MoveNextAsync())
        {
            values.AddRange(results.Current);
        }

        return values.ToArray();
    }
}