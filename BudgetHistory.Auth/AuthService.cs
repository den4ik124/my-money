using AutoMapper;
using BudgetHistory.Auth.Interfaces;
using BudgetHistory.Core.AppSettings;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using BudgetHistory.Core.Services.Responses;
using BudgetHistory.Logging;
using BudgetHistory.Logging.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetHistory.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly CustomLogger _logger;
        private readonly AuthTokenParameters _authTokenParameters;

        public AuthService(UserManager<IdentityUser> userManager,
                           SignInManager<IdentityUser> signInManager,
                           ITokenService tokenService,
                           IUnitOfWork unitOfWork,
                           IOptions<AuthTokenParameters> authParams,
                           IMapper mapper,
                           ICustomLoggerFactory logFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _authTokenParameters = authParams.Value;
            _logger = logFactory.CreateLogger<AuthService>();
        }

        public async Task<ServiceResponse> Authenticate(string userName, string password, HttpContext context)
        {
            try
            {
                var userFromDB = await _userManager.FindByNameAsync(userName);

                if (userFromDB == null)
                {
                    return await Failed($"User \"{userName}\" was not found.");
                    //var errorMessage = $"User \"{userName}\" was not found.";
                    //await _logger.LogError(errorMessage);
                    //return new AuthResult() { IsSuccess = false, Message = errorMessage };
                }

                var result = await _signInManager.CheckPasswordSignInAsync(userFromDB, password, false);
                if (result.Succeeded)
                {
                    var token = await _tokenService.CreateAuthTokenAsync(userFromDB);
                    context.Response.Cookies.Append(Cookies.ApplicationId, token,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTime.UtcNow.AddHours(_authTokenParameters.TokenExpirationTimeInHours),
                        SameSite = SameSiteMode.None,
                        Secure = true,
                    });
                    return ServiceResponse.Success("Successful login.");
                    //return new AuthResult() { IsSuccess = true, Message = "Successful login." };
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Method :{nameof(Authenticate)} has failed.\n{ex.Message}";
                await _logger.LogError(errorMessage);
            }

            return await Failed("Incorrect password.");
            //return new AuthResult() { IsSuccess = false, Message = "Incorrect password." };
        }

        public async Task<ServiceResponse> RegisterUser(IdentityUser identityUser, string password)
        {
            var errorMessage = string.Empty;
            var user = _mapper.Map<User>(identityUser);
            var result = await _userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
            {
                return await Failed(string.Join("|", result.Errors.Select(e => e.Description)));
                //errorMessage = string.Join("|", result.Errors.Select(e => e.Description));
                //await _logger.LogError(errorMessage);
                //return new AuthResult() { IsSuccess = result.Succeeded, Message = errorMessage };
            }

            var userFromDb = await _userManager.FindByNameAsync(identityUser.UserName);
            if (userFromDb == null)
            {
                return await Failed($"User ({identityUser.UserName}) does not exist yet.");
                //errorMessage = $"User ({identityUser.UserName}) does not exist yet.";
                //await _logger.LogError(errorMessage);
                //return new AuthResult() { IsSuccess = false, Message = errorMessage };
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(userFromDb, nameof(Roles.Customer));
            if (!addToRoleResult.Succeeded)
            {
                return await Failed(string.Join('|', addToRoleResult.Errors.Select(e => e.Description)));

                //errorMessage = string.Join('|', addToRoleResult.Errors.Select(e => e.Description));
                //await _logger.LogError(errorMessage);
                //return new AuthResult() { IsSuccess = false, Message = errorMessage };
            }

            user.Id = Guid.NewGuid();
            user.AssociatedIdentityUserId = new Guid(userFromDb.Id);

            if (await _unitOfWork.GetGenericRepository<User>().Add(user))
            {
                await _unitOfWork.CompleteAsync();
                return ServiceResponse.Success($"\"{identityUser.UserName}\" has been successfully registered.");

                //return new AuthResult() { IsSuccess = result.Succeeded, Message = $"\"{identityUser.UserName}\" has been successfully registered." };
            }
            return await Failed($"User ({identityUser.UserName}) can't be registered.");
            //errorMessage = $"User ({identityUser.UserName}) can't be registered.";
            //await _logger.LogError(errorMessage);
            //return new AuthResult() { IsSuccess = false, Message = errorMessage };
        }

        private async Task<ServiceResponse> Failed(string message)
        {
            var prefix = $"{nameof(AuthService)}:\n";
            await _logger.LogInfo(prefix + message);
            return ServiceResponse.Failure(message);
        }

        //private async Task<ServiceResponse<TResult>> Failed<TResult>(string message) where TResult : class
        //{
        //    var prefix = $"{nameof(AuthService)}:\n";
        //    await _logger.LogInfo(prefix + message);
        //    return ServiceResponse<TResult>.Failure(message);
        //}
    }
}