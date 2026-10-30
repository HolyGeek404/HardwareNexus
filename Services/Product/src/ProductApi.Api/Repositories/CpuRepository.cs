using MongoDB.Driver;
using ProductApi.Api.Models;
using ProductApi.Api.Repositories.Base;

namespace ProductApi.Api.Repositories;

public class CpuRepository(IMongoDatabase db) : MongoRepository<Cpu>(db)
{
    protected override string Category => ProductCategories.Cpu;

    public override async Task<object> GetFiltersAsync(CancellationToken ct = default)
    {
        var team = Distinct(c => c.Team, ct);
        var cores = Distinct(c => c.Cores, ct);
        var socket = Distinct(c => c.Socket, ct);
        var arch = Distinct(c => c.Architecture, ct);
        var tdp = Distinct(c => c.Tdp, ct);
        await Task.WhenAll(team, cores, socket, arch, tdp);

        return new CpuFilters
        {
            Team = team.Result, Cores = cores.Result, Socket = socket.Result,
            Architecture = arch.Result, TDP = tdp.Result
        };
    }
}