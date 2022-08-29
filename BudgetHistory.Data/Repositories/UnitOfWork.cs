using Notebook.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notebook.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly NotesDbContext context;
        private Dictionary<Type, IBaseRepository> reposiroties;

        public UnitOfWork(NotesDbContext context)
        {
            this.context = context;
            this.reposiroties = new Dictionary<Type, IBaseRepository>();
        }

        public async Task BeginTransactionAsync()
        {
            await this.context.Database.BeginTransactionAsync();
        }

        public void TransactionCommit()
        {
            this.context.Database.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            this.context.Database.RollbackTransaction();
        }

        public async Task<bool> CompleteAsync()
        {
            return await this.context.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            this.context.Dispose();
        }

        public IGenericRepository<TEntity> GetGenericRepository<TEntity>() where TEntity : class
        {
            if (this.reposiroties.ContainsKey(typeof(TEntity)))
            {
                return this.reposiroties[typeof(TEntity)] as GenericRepository<TEntity>;
            }

            var repo = new GenericRepository<TEntity>(this.context);
            this.reposiroties.Add(typeof(TEntity), repo);
            return repo;
        }
    }
}