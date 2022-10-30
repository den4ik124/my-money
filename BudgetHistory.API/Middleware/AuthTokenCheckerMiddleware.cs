using BudgetHistory.Core.Constants;
using BudgetHistory.Logging;
using BudgetHistory.Logging.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace BudgetHistory.API.Middleware
{
    public class AuthTokenCheckerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CustomLogger _logger;

        public AuthTokenCheckerMiddleware(RequestDelegate next,
                                         ICustomLoggerFactory log)
        {
            _next = next;
            _logger = log.CreateLogger<AuthTokenCheckerMiddleware>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = context.Request.Cookies[".AspNetCore.Application.Id"];
                if (!string.IsNullOrEmpty(token)
                    && string.IsNullOrEmpty(context.Request.Headers[Headers.Authorization]))
                {
                    context.Request.Headers.Add(Headers.Authorization, "Bearer " + token);
                }
                await _next(context);
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex.Message);
            }
        }
    }
}