namespace rbbl.buildingblocks.domain;

public abstract class BaseEntity<TId>
{
    public TId Id { get; protected set; } = default!;

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public string? ModifiedBy { get; set; }
}
