using MongoDB.Driver;
using ProductApi.Api.Interfaces;
using ProductApi.Api.Models;
using ProductApi.Api.Repositories.Base;

namespace ProductApi.Api.Repositories;

public class CoolerRepository(IMongoDatabase mongoDatabase) : MongoRepository<Cooler>(mongoDatabase), ICoolerRepository;