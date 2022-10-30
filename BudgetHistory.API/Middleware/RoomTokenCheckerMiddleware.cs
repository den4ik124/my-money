using BudgetHistory.Core.Constants;
using BudgetHistory.Logging;
using BudgetHistory.Logging.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace BudgetHistory.API.Middleware
{
    public class RoomTokenCheckerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CustomLogger _logger;

        public RoomTokenCheckerMiddleware(RequestDelegate next,
                                           ICustomLoggerFactory logFactory)
        {
            _next = next;
            _logger = logFactory.CreateLogger<RoomTokenCheckerMiddleware>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = context.Request.Cookies[Cookies.RoomAuth];
                if (!string.IsNullOrEmpty(token)
                    && string.IsNullOrEmpty(context.Request.Headers[Headers.RoomAuthorization]))
                {
                    context.Request.Headers.Add(Headers.RoomAuthorization, "Bearer " + token);
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