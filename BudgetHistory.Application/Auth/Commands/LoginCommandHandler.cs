using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Auth.Interfaces;
using BudgetHistory.Core.AppSettings;
using BudgetHistory.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Auth.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<string>>
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly TokenService tokenService;
        private readonly IAuthService authService;
        private readonly IMapper mapper;
        private readonly AuthTokenParameters authParams;

        public LoginCommandHandler(UserManager<IdentityUser> userManager,
                                   SignInManager<IdentityUser> signInManager,
                                   TokenService tokenService,
                                   IOptions<AuthTokenParameters> authParams,
                                   IAuthService authService,
                                   IMapper mapper)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.tokenService = tokenService;
            this.authService = authService;
            this.mapper = mapper;
            this.authParams = authParams.Value;
        }

        public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserLoginDto.UserName))
            {
                return Result<string>.Failure("Username has not been inputted!");
            }

            var result = await authService.Authenticate(request.UserLoginDto.UserName, request.UserLoginDto.Password, request.HttpContext);

            return result.IsSuccess ? Result<string>.Success(result.Message) : Result<string>.Failure(result.Message);
        }
    }
}