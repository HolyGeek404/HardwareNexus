using ProductApi.Api.Models;

namespace ProductApi.Api.Interfaces;

public interface IGpuRepository : IReadRepository<Gpu>, IWriteRepository<Gpu>;