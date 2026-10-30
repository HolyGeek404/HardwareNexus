using System.Linq.Expressions;
using System.Text.Json;
using MongoDB.Driver;
using ProductApi.Api.Interfaces;
using ProductApi.Api.Models;

namespace ProductApi.Api.Repositories.Base;

public abstract class MongoRepository<T>(IMongoDatabase db) : IProductRepository
    where T : BaseProduct
{
    private IMongoCollection<T> Collection => db.GetCollection<T>("products");
    protected abstract string Category { get; }

    public async Task<IReadOnlyList<object>> GetByType(CancellationToken ct = default)
    {
        var items = await Collection.Find(p => p.Category == Category).ToListAsync(ct);
        return items.Cast<object>().ToList();
    }

    public async Task<object?> GetById(string id, CancellationToken ct = default) =>
        await Collection.Find(p => p.Category == Category && p.ProductId == id)
            .FirstOrDefaultAsync(ct);

    public async Task CreateAsync(JsonElement json, CancellationToken ct = default)
    {
        var product = json.Deserialize<T>(JsonOptions)
                      ?? throw new JsonException($"Invalid {typeof(T).Name} payload.");
        await Collection.InsertOneAsync(product, cancellationToken: ct);
    }

    public async Task<bool> UpdateAsync(JsonElement json, CancellationToken ct = default)
    {
        var product = json.Deserialize<T>(JsonOptions)
                      ?? throw new JsonException($"Invalid {typeof(T).Name} payload.");
        var result = await Collection.ReplaceOneAsync(
            p => p.Category == Category && p.ProductGuid == product.ProductGuid,
            product, cancellationToken: ct);
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var result = await Collection.DeleteOneAsync(
            p => p.Category == Category && p.ProductGuid == id.ToString(), ct);
        return result.DeletedCount > 0;
    }

    // only thing each category must supply
    public abstract Task<object> GetFiltersAsync(CancellationToken ct = default);

    protected async Task<TField[]> Distinct<TField>(
        Expression<Func<T, TField>> field, CancellationToken ct = default)
    {
        var cursor = await Collection.DistinctAsync(field, p => p.Category == Category, cancellationToken: ct);
        return (await cursor.ToListAsync(ct)).ToArray();
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}