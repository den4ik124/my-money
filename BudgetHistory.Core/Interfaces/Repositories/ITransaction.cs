using System.Threading.Tasks;

namespace Notebook.Core.Interfaces.Repositories
{
    public interface ITransaction
    {
        Task BeginTransactionAsync();

        void TransactionCommit();

        void RollbackTransaction();
    }
}