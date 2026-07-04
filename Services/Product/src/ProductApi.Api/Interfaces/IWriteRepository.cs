using System.Net;
using ProductApi.Api.Models;

namespace ProductApi.Api.Interfaces;

public interface IWriteRepository<TProduct>
{
    public Task<BaseProduct?> CreateAsync(TProduct entity, string id, string pk);
    public Task<HttpStatusCode> UpdateAsync(TProduct entity, string id, string pk);
    public Task<HttpStatusCode> DeleteAsync(Guid id, string partitionKey);
}