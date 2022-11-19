using System.Threading.Tasks;

namespace BudgetHistory.Abstractions.Interfaces.Data
{
    public interface ITransaction
    {
        Task BeginTransactionAsync();

        void TransactionCommit();

        void RollbackTransaction();
    }
}