using BudgetHistory.Logging.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BudgetHistory.Logging
{
    public class CustomLoggerFactory : ICustomLoggerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _config;

        public CustomLoggerFactory(ILoggerFactory loggerFactory, IConfiguration config)
        {
            _loggerFactory = loggerFactory;
            _config = config;
        }

        public CustomLogger CreateLogger<T>()
        {
            var loggersList = new List<ICustomLogger>()
            {
                new TelegramLogger(_config),
            };
            return new CustomLogger(_loggerFactory.CreateLogger<T>()) { CustomLoggers = loggersList };
        }
    }
}