using System.Linq.Expressions;
using rbbl.buildingblocks.DomainDriven;

namespace rbbl.buildingblocks.Abstractions;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
    IQueryable<T> Query(Expression<Func<T, bool>>? predicate = null);
}
