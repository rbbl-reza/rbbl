namespace rbbl.buildingblocks.domain;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}