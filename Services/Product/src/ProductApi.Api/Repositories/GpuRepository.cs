using Microsoft.Azure.Cosmos;
using ProductApi.Api.Interfaces;
using ProductApi.Api.Models;

namespace ProductApi.Api.Repositories;

public class GpuRepository(CosmosClient cosmosClient) : CosmosRepository<Gpu>(cosmosClient), IGpuRepository
{
}