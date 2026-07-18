using MongoDB.Driver;
using ProductApi.Api.Interfaces;
using ProductApi.Api.Models;
using ProductApi.Api.Repositories.Base;

namespace ProductApi.Api.Repositories;

public class GpuRepository(IMongoDatabase mongoDatabase) : MongoRepository<Gpu>(mongoDatabase), IGpuRepository
{
}