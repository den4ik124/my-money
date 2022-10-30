using BudgetHistory.Logging;
using BudgetHistory.Logging.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetHistory.API.Extensions
{
    public static class LoggingExtensions
    {
        public static IServiceCollection AddCustomLoggers(this IServiceCollection services)
        {
            services.AddSingleton<ICustomLoggerFactory, CustomLoggerFactory>();

            //Add custom loogers below
            services.AddSingleton<ITgLogger, TelegramLogger>();

            return services;
        }
    }
}