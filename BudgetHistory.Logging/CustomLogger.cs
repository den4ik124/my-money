using BudgetHistory.Logging.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetHistory.Logging
{
    public class CustomLogger
    {
        private readonly ILogger _defaultLogger;

        public CustomLogger(ILogger defaultLogger)
        {
            _defaultLogger = defaultLogger;
        }

        public IEnumerable<ICustomLogger> CustomLoggers { get; set; }

        public async Task LogError(string errorMessage)
        {
            _defaultLogger.LogError(errorMessage);
            if (CustomLoggers.Any())
            {
                await Task.Run(() =>
                {
                    foreach (var logger in CustomLoggers)
                    {
                        logger.LogError(errorMessage);
                    }
                });
            }
        }

        public async Task LogInfo(string infoMessage)
        {
            _defaultLogger.LogInformation(infoMessage);
            if (CustomLoggers.Any())
            {
                await Task.Run(() =>
                {
                    foreach (var logger in CustomLoggers)
                    {
                        logger.LogInfo(infoMessage);
                    }
                });
            }
        }
    }
}