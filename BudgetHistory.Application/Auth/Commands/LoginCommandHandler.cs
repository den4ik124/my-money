using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Notebook.Application.Core;
using Notebook.Application.DTOs.Auth;
using Notebook.Core.AppSettings;
using Notebook.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Notebook.Application.Auth.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<object>>
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly TokenService tokenService;
        private readonly AuthTokenParameters authParams;

        public LoginCommandHandler(UserManager<IdentityUser> userManager,
                                   SignInManager<IdentityUser> signInManager,
                                   TokenService tokenService,
                                   IOptions<AuthTokenParameters> authParams)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.tokenService = tokenService;
            this.authParams = authParams.Value;
        }

        public async Task<Result<object>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            IdentityUser userFromDb = null;

            if (string.IsNullOrEmpty(request.UserLoginDto.UserName))
            {
                return Result<object>.Failure("Username has not been inputted!");
            }

            if (!string.IsNullOrEmpty(request.UserLoginDto.UserName))
            {
                userFromDb = await this.userManager.FindByNameAsync(request.UserLoginDto.UserName);
                if (userFromDb == null)
                {
                    return Result<object>.Failure("User has not been found");
                }
            }

            var loginResult = await this.signInManager.CheckPasswordSignInAsync(userFromDb,
                                                                                request.UserLoginDto.Password,
                                                                                lockoutOnFailure: false);
            if (loginResult != null && loginResult.Succeeded)
            {
                var userData = await GetUserDto(userFromDb, request.HttpContext);
                return Result<object>.Success(userData);
            }
            else
            {
                return Result<object>.Failure("Login or password is invalid!");
            }
        }

        private async Task<UserDataDto> GetUserDto(IdentityUser user, HttpContext context, bool addTokenIntoHeaders = true)
        {
            var userDto = new UserDataDto()
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = await this.userManager.GetRolesAsync(user)
            };

            if (addTokenIntoHeaders)
            {
                var token = await this.tokenService.CreateToken(user);
                context.Response.Cookies.Append(".AspNetCore.Application.Id", token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddHours(authParams.TokenExpirationTimeInHours),
                    SameSite = SameSiteMode.None,
                    Secure = true,
                });
            }

            return userDto;
        }
    }
}