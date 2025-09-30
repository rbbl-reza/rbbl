namespace rbbl.buildingblocks.DomainDriven;

public abstract class BaseEntity : IHasDomainEvents
{
    private readonly List<DomainEvent> _domainEvents = [];

    /// <summary>
    /// Primary key identifier for the entity.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// UTC timestamp when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the user who created this entity.
    /// </summary>
    public string? CreatedBy { get; protected set; }

    /// <summary>
    /// UTC timestamp when the entity was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; protected set; }

    /// <summary>
    /// Identifier of the user who last modified this entity.
    /// </summary>
    public string? ModifiedBy { get; protected set; }

    /// <summary>
    /// Collection of domain events raised by this entity.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raise a new domain event.
    /// </summary>
    protected void RaiseDomainEvent(DomainEvent @event) => _domainEvents.Add(@event);

    // <summary>
    /// Clear all domain events (after dispatch).
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
    
    /// <summary>
    /// Set the creator information.
    /// Usually called when the entity is first persisted.
    /// </summary>
    public void SetCreated(string createdBy)
    {
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark the entity as updated.
    /// </summary>
    public void SetModified(string? modifiedBy = null)
    {
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }
} 
