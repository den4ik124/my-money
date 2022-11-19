using AutoMapper;
using BudgetHistory.Abstractions.Interfaces.Data;
using BudgetHistory.Abstractions.Services;
using BudgetHistory.Auth.Interfaces;
using BudgetHistory.Business.Services;
using BudgetHistory.Core.AppSettings;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Models;
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
    public class AuthService : BaseService, IAuthService
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
                    return await Failed(_logger, $"User \"{userName}\" was not found.");
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
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Method :{nameof(Authenticate)} has failed.\n{ex.Message}";
                await _logger.LogError(errorMessage);
            }

            return await Failed(_logger, "Incorrect password.");
        }

        public async Task<ServiceResponse> RegisterUser(IdentityUser identityUser, string password)
        {
            var errorMessage = string.Empty;
            var user = _mapper.Map<User>(identityUser);
            var result = await _userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
            {
                return await Failed(_logger, string.Join("|", result.Errors.Select(e => e.Description)));
            }

            var userFromDb = await _userManager.FindByNameAsync(identityUser.UserName);
            if (userFromDb == null)
            {
                return await Failed(_logger, $"User ({identityUser.UserName}) does not exist yet.");
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(userFromDb, nameof(Roles.Customer));
            if (!addToRoleResult.Succeeded)
            {
                return await Failed(_logger, string.Join('|', addToRoleResult.Errors.Select(e => e.Description)));
            }

            user.Id = Guid.NewGuid();
            user.AssociatedIdentityUserId = new Guid(userFromDb.Id);

            if (await _unitOfWork.GetGenericRepository<User>().Add(user))
            {
                await _unitOfWork.CompleteAsync();
                return ServiceResponse.Success($"\"{identityUser.UserName}\" has been successfully registered.");
            }
            return await Failed(_logger, $"User ({identityUser.UserName}) can't be registered.");
        }
    }
}