using Microsoft.Azure.Cosmos;
using ProductApi.Api.Interfaces;
using ProductApi.Api.Models;

namespace ProductApi.Api.Repositories;

public class CoolerRepository(CosmosClient cosmosClient) : CosmosRepository<Cooler>(cosmosClient), ICoolerRepository;