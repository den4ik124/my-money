using System.Threading.Tasks;

namespace BudgetHistory.Logging.Interfaces
{
    public interface ICustomLogger
    {
        Task LogError(string errorMessage);

        Task LogInfo(string infoMessage);
    }
}