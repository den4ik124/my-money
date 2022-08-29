using Microsoft.EntityFrameworkCore;
using Notebook.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Notebook.Data.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly NotesDbContext context;
        protected readonly DbSet<TEntity> dbSet;

        public GenericRepository(NotesDbContext context)
        {
            this.context = context;
            this.dbSet = this.context.Set<TEntity>();
        }

        public virtual async Task<bool> Add(TEntity entity)
        {
            await this.dbSet.AddAsync(entity);
            return true;
        }

        public virtual async Task<bool> AddRange(IEnumerable<TEntity> entities)
        {
            await this.dbSet.AddRangeAsync(entities);
            return true;
        }

        public virtual bool Delete(TEntity entity)
        {
            this.dbSet.Remove(entity);
            this.context.Entry(entity).State = EntityState.Deleted;
            return true;
        }

        public async Task<bool> DeleteById<TId>(TId id)
        {
            var entity = await this.dbSet.FindAsync(id);
            return Delete(entity);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAll()
        {
            return await this.dbSet.ToListAsync();
        }

        public virtual async Task<TEntity> GetById<TId>(TId id)
        {
            return await this.dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetWhere(Expression<Func<TEntity, bool>> predicate)
        {
            return await this.dbSet.Where(predicate).ToListAsync();
        }

        public async Task<int> GetItemsCount(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate != null)
            {
                return await this.dbSet.Where(predicate).CountAsync();
            }
            return await this.dbSet.CountAsync();
        }

        public virtual bool Update(TEntity entity)
        {
            this.dbSet.Attach(entity);
            this.context.Entry(entity).State = EntityState.Modified;
            return true;
        }

        public IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate = null,
                                                     Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
        {
            IQueryable<TEntity> query = this.dbSet.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            if (orderBy != null)
            {
                return orderBy(query);
            }
            return query;
        }
    }
}