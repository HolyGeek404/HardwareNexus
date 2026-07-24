using System.Net;
using MongoDB.Driver;
using ProductApi.Api.Interfaces;
using ProductApi.Api.Models;

namespace ProductApi.Api.Repositories.Base;

public class MongoRepository<TProduct>(IMongoDatabase database) : IReadRepository<TProduct>, IWriteRepository<TProduct>
    where TProduct : BaseProduct
{
    protected readonly IMongoCollection<TProduct> Collection = database.GetCollection<TProduct>("products");

    public async Task<IEnumerable<TProduct>> GetByType(string category)
    {
        var filter = Builders<TProduct>.Filter.Eq(p => p.Category, category);
        var results = await Collection.Find(filter).ToListAsync();
        return results;
    }

    public async Task<TProduct?> GetById(string category, string id)
    {
        var filter = Builders<TProduct>.Filter.And(
            Builders<TProduct>.Filter.Eq(p => p.Category, category),
            Builders<TProduct>.Filter.Eq(p => p.ProductId, id));

        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<BaseProduct?> CreateAsync(TProduct entity, string id, string pk)
    {
        try
        {
            await Collection.InsertOneAsync(entity);
            return entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<HttpStatusCode> UpdateAsync(TProduct entity, string id, string pk)
    {
        try
        {
            var filter = Builders<TProduct>.Filter.Eq(p => p.ProductGuid, id);
            var result = await Collection.ReplaceOneAsync(filter, entity);

            if (result.MatchedCount == 0)
                return HttpStatusCode.NotFound;

            return HttpStatusCode.OK;
        }
        catch (Exception)
        {
            return HttpStatusCode.NotFound;
        }
    }

    public async Task<HttpStatusCode> DeleteAsync(Guid id, string partitionKey)
    {
        var filter = Builders<TProduct>.Filter.Eq(p => p.ProductGuid, id.ToString());
        var result = await Collection.DeleteOneAsync(filter);

        return result.DeletedCount > 0
            ? HttpStatusCode.OK
            : HttpStatusCode.NotFound;
    }
}