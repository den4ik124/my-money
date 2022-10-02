using BudgetHistory.Core.Constants;
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

        public RoomTokenCheckerMiddleware(RequestDelegate next,
                                          ILogger<RoomTokenCheckerMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = context.Request.Cookies[Cookies.RoomAuth];
                if (!string.IsNullOrEmpty(token)
                    && string.IsNullOrEmpty(context.Request.Headers[Headers.RoomAuthorization]))
                {
                    context.Request.Headers.Add(Headers.RoomAuthorization, "RoomAuth " + token);
                }
                await this.next(context);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
            }
        }
    }
}