using AutoMapper;
using BudgetHistory.Auth.Interfaces;
using BudgetHistory.Core.AppSettings;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BudgetHistory.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ILogger<AuthService> logger;
        private readonly IMapper mapper;
        private readonly TokenService tokenService;
        private readonly IUnitOfWork unitOfWork;
        private readonly AuthTokenParameters authTokenParameters;

        public AuthService(UserManager<IdentityUser> userManager,
                           SignInManager<IdentityUser> signInManager,
                           TokenService tokenService,
                           IUnitOfWork unitOfWork,
                           IOptions<AuthTokenParameters> authParams,
                           ILogger<AuthService> logger,
                           IMapper mapper)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.mapper = mapper;
            this.tokenService = tokenService;
            this.unitOfWork = unitOfWork;
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
                var token = await this.tokenService.CreateAuthTokenAsync(userFromDB);
                context.Response.Cookies.Append(Cookies.ApplicationId, token,
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

        public async Task<AuthResult> RegisterUser(IdentityUser identityUser, string password)
        {
            var errorMessage = string.Empty;
            var user = mapper.Map<User>(identityUser);
            var result = await userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
            {
                errorMessage = string.Join("|", result.Errors.Select(e => e.Description));
                logger.LogError(errorMessage);
                return new AuthResult() { IsSuccess = result.Succeeded, Message = errorMessage };
            }

            var userFromDb = await userManager.FindByNameAsync(identityUser.UserName);
            if (userFromDb == null)
            {
                errorMessage = $"User ({identityUser.UserName}) does not exist yet.";
                logger.LogError(errorMessage);
                return new AuthResult() { IsSuccess = false, Message = errorMessage };
            }

            var addToRoleResult = await userManager.AddToRoleAsync(userFromDb, nameof(Roles.Customer));
            if (!addToRoleResult.Succeeded)
            {
                errorMessage = string.Join('|', addToRoleResult.Errors.Select(e => e.Description));
                logger.LogError(errorMessage);
                return new AuthResult() { IsSuccess = false, Message = errorMessage };
            }

            user.Id = Guid.NewGuid();
            user.AssociatedIdentityUserId = new Guid(userFromDb.Id);

            if (await unitOfWork.GetGenericRepository<User>().Add(user))
            {
                await unitOfWork.CompleteAsync();
                return new AuthResult() { IsSuccess = result.Succeeded, Message = $"\"{identityUser.UserName}\" has been successfully registered." };
            }

            errorMessage = $"User ({identityUser.UserName}) can't be registered.";
            logger.LogError(errorMessage);
            return new AuthResult() { IsSuccess = false, Message = errorMessage };
        }

        public IEnumerable<Claim> DecodeToken(string authToken)
        {
            var jwtToken = authToken.Split(" ").Last();
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(jwtToken).Claims;
        }
    }
}