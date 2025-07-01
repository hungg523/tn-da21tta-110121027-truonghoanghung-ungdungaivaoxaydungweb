using AppleShop.Domain.Abstractions.Common;
using AppleShop.Domain.Abstractions.IRepositories.Base;
using AppleShop.Share.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Linq.Expressions;

namespace AppleShop.Infrastructure.Repositories.Base
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly ApplicationDbContext dbContext;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Delete(T entity)
        {
            dbContext.Set<T>().Remove(entity);
        }

        public void Update(T entity)
        {
            dbContext.Entry(entity).State = EntityState.Modified;
        }

        public void Create(T entity)
        {
            dbContext.Add(entity);
        }

        public IQueryable<T> FindAll(Expression<Func<T, bool>>? predicate = null, bool isTracking = false, params Expression<Func<T, object>>[] includeProperties)
        {
            var query = dbContext.Set<T>().AsQueryable();
            if (includeProperties.Any())
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }
            query = isTracking ? query : query.AsNoTracking();
            return predicate is not null ? query.Where(predicate) : query;
        }

        public IQueryable<T> FindAllWithPaging(PaginationQuery queryObject, Expression<Func<T, bool>>? predicate = null, bool isTracking = false, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = dbContext.Set<T>().AsQueryable();
            if (includeProperties.Any())
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            query = isTracking ? query : query.AsNoTracking();
            query = predicate != null ? query.Where(predicate) : query;
            query = query.Skip((queryObject.PageNumber.Value - 1) * queryObject.PageSize.Value).Take(queryObject.PageSize.Value);

            if (!string.IsNullOrWhiteSpace(queryObject.SortBy))
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var property = Expression.Property(parameter, queryObject.SortBy);
                var lambda = Expression.Lambda(property, parameter);

                var method = queryObject.IsDescending!.Value ? "OrderByDescending" : "OrderBy";

                var resultExpression = Expression.Call(
                    typeof(Queryable),
                    method,
                    new Type[] { query.ElementType, property.Type },
                    query.Expression,
                    lambda
                );

                query = query.Provider.CreateQuery<T>(resultExpression);
            }

            return query;
        }

        public async Task<T?> FindByIdAsync(object id, bool isTracking = false, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Set<T>().AsQueryable();
            query = isTracking ? query : query.AsNoTracking();

            var keyName = dbContext.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.First().Name;
            var result = await query.FirstOrDefaultAsync(x => EF.Property<object>(x, keyName).Equals(id), cancellationToken);
            return result;
        }

        public async Task<List<T?>> FindByIds(IEnumerable<int> ids, bool isTracking = false, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties)
        {
            var query = dbContext.Set<T>().AsQueryable();

            if (includeProperties.Any())
                query = IncludeMultiple(query, includeProperties);

            query = isTracking ? query : query.AsNoTracking();

            var result = await query.Where(x => ids.Contains(x.Id.Value)).ToListAsync(cancellationToken);
            return result;
        }

        private IQueryable<T> IncludeMultiple(IQueryable<T> source, params Expression<Func<T, object>>[] includeProperties)
        {
            if (includeProperties.Any())
                // Each property will be included into source
                source = includeProperties.Aggregate(source, (current, include) => current.Include(include));
            return source;
        }

        public async Task<T?> FindSingleAsync(Expression<Func<T, bool>> predicate, bool isTracking = false, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties)
        {
            var query = dbContext.Set<T>().AsQueryable();
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            query = isTracking ? query : query.AsNoTracking();
            var result = predicate is not null ? await query.FirstOrDefaultAsync(predicate, cancellationToken) : await query.FirstOrDefaultAsync(cancellationToken);
            return result;
        }

        public void RemoveMultiple(IEnumerable<T> entities)
        {
            dbContext.Set<T>().RemoveRange(entities);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            dbContext.Set<T>().AddRange(entities);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            dbContext.Set<T>().UpdateRange(entities);
        }

        public int Count(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = dbContext.Set<T>();
            return predicate is not null ? query.Where(predicate).Count() : query.Count();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);

        public async Task ExecuteSqlRawAsync(string sql, params object[] parameters)
        {
            await dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            return transaction.GetDbTransaction();
        }
    }
}