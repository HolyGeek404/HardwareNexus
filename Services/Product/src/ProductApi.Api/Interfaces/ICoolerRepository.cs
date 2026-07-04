using ProductApi.Api.Models;

namespace ProductApi.Api.Interfaces;

public interface ICoolerRepository : IReadRepository<Cooler>, IWriteRepository<Cooler>;