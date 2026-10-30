using System.Text.Json;

namespace ProductApi.Api.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<object>> GetByType(CancellationToken ct = default);
    Task<object?> GetById(string id, CancellationToken ct = default);
    Task<object> GetFiltersAsync(CancellationToken ct = default);
    Task CreateAsync(JsonElement json, CancellationToken ct = default);
    Task<bool> UpdateAsync(JsonElement json, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}