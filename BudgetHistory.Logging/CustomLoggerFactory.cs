using BudgetHistory.Logging.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BudgetHistory.Logging
{
    public class CustomLoggerFactory : ICustomLoggerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ITgLogger _tgLogger;

        public CustomLoggerFactory(ILoggerFactory loggerFactory, ITgLogger tgLogger)
        {
            _loggerFactory = loggerFactory;
            _tgLogger = tgLogger;
        }

        public CustomLogger CreateLogger<T>()
        {
            var loggersList = new List<ICustomLogger>()
            {
                _tgLogger,
            };
            return new CustomLogger(_loggerFactory.CreateLogger<T>()) { CustomLoggers = loggersList };
        }
    }
}