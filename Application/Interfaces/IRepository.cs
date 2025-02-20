using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{

    public interface IRepository<T> where T : IAggregateRoot
    {
        Task AddAsync(T entity, CancellationToken cancellationToken = default(CancellationToken));

        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default(CancellationToken));

        Task UpdateAsync(T entity, CancellationToken cancellationToken = default(CancellationToken));

        Task RemoveAsync(T entity, CancellationToken cancellationToken = default(CancellationToken));

        Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));

        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken));

        Task<T> FindAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));
    }
}
