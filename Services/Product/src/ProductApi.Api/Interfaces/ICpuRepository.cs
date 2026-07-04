using ProductApi.Api.Models;

namespace ProductApi.Api.Interfaces;

public interface ICpuRepository : IReadRepository<Cpu>, IWriteRepository<Cpu>
{
    Task<CpuFilters> GetFiltersAsync(string category);
}
