using BudgetHistory.Core.Services.Responses;
using BudgetHistory.Logging;
using System.Threading.Tasks;

namespace BudgetHistory.Core.Services
{
    public class BaseService
    {
        protected async Task<ServiceResponse> Failed(CustomLogger logger, string message)
        {
            var prefix = $"{nameof(NoteService)}:\n";
            await logger.LogInfo(prefix + message);
            return ServiceResponse.Failure(message);
        }

        protected async Task<ServiceResponse<TResult>> Failed<TResult>(CustomLogger logger, string message) where TResult : class
        {
            var prefix = $"{nameof(NoteService)}:\n";
            await logger.LogInfo(prefix + message);
            return ServiceResponse<TResult>.Failure(message);
        }
    }
}