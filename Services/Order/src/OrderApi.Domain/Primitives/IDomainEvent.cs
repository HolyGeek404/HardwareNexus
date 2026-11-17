namespace OrderApi.Domain.Primitives;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
