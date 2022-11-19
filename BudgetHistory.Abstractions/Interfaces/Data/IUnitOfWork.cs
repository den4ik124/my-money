using System.Threading.Tasks;

namespace BudgetHistory.Abstractions.Interfaces.Data
{
    public interface IUnitOfWork : ITransaction
    {
        IGenericRepository<TEntity> GetGenericRepository<TEntity>() where TEntity : class;

        Task<bool> CompleteAsync();
    }
}