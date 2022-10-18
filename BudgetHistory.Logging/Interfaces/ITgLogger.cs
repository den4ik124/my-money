using System.Threading.Tasks;

namespace BudgetHistory.Logging.Interfaces
{
    public interface ITgLogger
    {
        Task LogError(string errorMessage);

        Task LogInfo(string infoMessage);
    }
}