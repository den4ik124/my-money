using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Notebook.Core.Interfaces.Repositories
{
    public interface IGenericRepository<TEntity> : IBaseRepository where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAll();

        IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null);

        Task<TEntity> GetById<TId>(TId id);

        Task<IEnumerable<TEntity>> GetWhere(Expression<Func<TEntity, bool>> predicate);

        Task<int> GetItemsCount(Expression<Func<TEntity, bool>> predicate = null);

        Task<bool> Add(TEntity entity);

        Task<bool> AddRange(IEnumerable<TEntity> entities);

        bool Update(TEntity entity);

        bool Delete(TEntity entity);

        Task<bool> DeleteById<TId>(TId id);
    }
}