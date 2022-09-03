using BudgetHistory.Auth.Interfaces;
using BudgetHistory.Core.AppSettings;
using BudgetHistory.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace BudgetHistory.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ILogger<AuthService> logger;
        private readonly TokenService tokenService;
        private readonly AuthTokenParameters authTokenParameters;

        public AuthService(UserManager<IdentityUser> userManager,
                           SignInManager<IdentityUser> signInManager,
                           ILogger<AuthService> logger,
                           TokenService tokenService,
                           IOptions<AuthTokenParameters> authParams)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.tokenService = tokenService;
            this.authTokenParameters = authParams.Value;
        }

        public async Task<AuthResult> Authenticate(string userName, string password, HttpContext context)
        {
            var userFromDB = await userManager.FindByNameAsync(userName);
            if (userFromDB == null)
            {
                var errorMessage = $"User \"{userName}\" was mot found.";
                logger.LogError(errorMessage);
                return new AuthResult() { IsSuccess = false, Message = errorMessage };
            }

            var result = await signInManager.CheckPasswordSignInAsync(userFromDB, password, false);
            if (result.Succeeded)
            {
                var token = await this.tokenService.CreateToken(userFromDB);
                context.Response.Cookies.Append(".AspNetCore.Application.Id", token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddHours(authTokenParameters.TokenExpirationTimeInHours),
                    SameSite = SameSiteMode.None,
                    Secure = true,
                });
                return new AuthResult() { IsSuccess = true, Message = "Successful login." };
            }
            return new AuthResult() { IsSuccess = false, Message = "Incorrect password." };
        }
    }
}