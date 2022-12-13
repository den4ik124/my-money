using BudgetHistory.Core.Services.Responses;
using BudgetHistory.Logging;
using System.Threading.Tasks;

namespace BudgetHistory.Business.Services
{
    public class BaseService
    {
        protected async Task<ServiceResponse> Failed<TService>(CustomLogger logger, string message)
        {
            var prefix = $"{typeof(TService).Name}:\n";
            await logger.LogInfo(prefix + message);
            return ServiceResponse.Failure(message);
        }

        protected async Task<ServiceResponse<TResult>> Failed<TService, TResult>(CustomLogger logger, string message) where TResult : class
        {
            var prefix = $"{typeof(TService).Name}:\n";
            await logger.LogInfo(prefix + message);
            return ServiceResponse<TResult>.Failure(message);
        }
    }
}