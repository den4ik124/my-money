using BudgetHistory.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BudgetHistory.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly NotesDbContext _context;
        private readonly Dictionary<Type, IBaseRepository> _repositories;

        public UnitOfWork(NotesDbContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, IBaseRepository>();
        }

        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public void TransactionCommit()
        {
            _context.Database.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            _context.Database.RollbackTransaction();
        }

        public async Task<bool> CompleteAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public IGenericRepository<TEntity> GetGenericRepository<TEntity>() where TEntity : class
        {
            if (_repositories.ContainsKey(typeof(TEntity)))
            {
                return _repositories[typeof(TEntity)] as GenericRepository<TEntity>;
            }

            var repo = new GenericRepository<TEntity>(_context);
            _repositories.Add(typeof(TEntity), repo);
            return repo;
        }
    }
}