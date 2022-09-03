using System.Threading.Tasks;

namespace BudgetHistory.Core.Interfaces.Repositories
{
    public interface IUnitOfWork : ITransaction
    {
        IGenericRepository<TEntity> GetGenericRepository<TEntity>() where TEntity : class;

        Task<bool> CompleteAsync();
    }
}