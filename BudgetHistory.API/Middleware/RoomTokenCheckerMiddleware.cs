using BudgetHistory.Core.Constants;
using BudgetHistory.Logging.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BudgetHistory.API.Middleware
{
    public class RoomTokenCheckerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RoomTokenCheckerMiddleware> logger;
        private readonly ITgLogger tgLogger;

        public RoomTokenCheckerMiddleware(RequestDelegate next,
                                          ILogger<RoomTokenCheckerMiddleware> logger,
                                          ITgLogger tgLogger)
        {
            this.next = next;
            this.logger = logger;
            this.tgLogger = tgLogger;
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
                await this.next(context);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                await this.tgLogger.LogError(ex.Message);
            }
        }
    }
}