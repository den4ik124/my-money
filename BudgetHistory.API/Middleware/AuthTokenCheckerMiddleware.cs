using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Notebook.Core.Constants;
using System;
using System.Threading.Tasks;

namespace Notebook.API.Middleware
{
    public class AuthTokenCheckerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionMiddleware> logger;

        public AuthTokenCheckerMiddleware(RequestDelegate next,
                                          ILogger<ExceptionMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
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
                await this.next(context);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
            }
        }
    }
}