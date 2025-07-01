using AppleShop.Domain.Abstractions.Common;
using AppleShop.Share.Shared;
using System.Data;
using System.Linq.Expressions;

namespace AppleShop.Domain.Abstractions.IRepositories.Base
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
        void AddRange(IEnumerable<T> entities);
        void UpdateRange(IEnumerable<T> entities);
        void RemoveMultiple(IEnumerable<T> entities);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task ExecuteSqlRawAsync(string sql, params object[] parameters);
        IQueryable<T> FindAll(Expression<Func<T, bool>>? predicate = null, bool isTracking = false, params Expression<Func<T, object>>[] includeProperties);
        IQueryable<T> FindAllWithPaging(PaginationQuery queryObject, Expression<Func<T, bool>>? predicate = null, bool isTracking = false, params Expression<Func<T, object>>[] includeProperties);
        Task<T?> FindByIdAsync(object id, bool isTracking = false, CancellationToken cancellationToken = default);
        Task<List<T?>> FindByIds(IEnumerable<int> ids, bool isTracking = false, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties);
        Task<T?> FindSingleAsync(Expression<Func<T, bool>> predicate, bool isTracking = false, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties);
        Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}