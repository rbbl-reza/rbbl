namespace rbbl.buildingblocks.abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}