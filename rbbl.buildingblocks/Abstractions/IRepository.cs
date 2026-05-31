using System.Linq.Expressions;
using rbbl.buildingblocks.domain;

namespace rbbl.buildingblocks.abstractions;

public interface IRepository<TId> where TId : BaseEntity<TId>
{
    Task<TId?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TId entity, CancellationToken ct = default);
    Task UpdateAsync(TId entity, CancellationToken ct = default);
    Task DeleteAsync(TId entity, CancellationToken ct = default);
    IQueryable<TId> Query(Expression<Func<TId, bool>>? predicate = null);
}
