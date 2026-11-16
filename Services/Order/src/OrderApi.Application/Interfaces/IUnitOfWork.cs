using OrderApi.Domain.Primitives;

namespace OrderApi.Application.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}