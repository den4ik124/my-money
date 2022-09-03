using System.Threading.Tasks;

namespace BudgetHistory.Core.Interfaces.Repositories
{
    public interface ITransaction
    {
        Task BeginTransactionAsync();

        void TransactionCommit();

        void RollbackTransaction();
    }
}