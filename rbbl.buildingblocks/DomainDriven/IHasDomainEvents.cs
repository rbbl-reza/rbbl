namespace rbbl.buildingblocks.DomainDriven;

/// <summary>
/// Entities that raise domain events implement this.
/// BaseEntity already implements most of the mechanics,
/// but this interface allows generic handling (e.g. in a dispatcher).
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>All domain events raised by the entity.</summary>
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }

    /// <summary>Clear domain events once they’ve been dispatched.</summary>
    void ClearDomainEvents();
}
