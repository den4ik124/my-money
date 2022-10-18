using BudgetHistory.Application.Core;
using BudgetHistory.Auth.Interfaces;
using BudgetHistory.Logging.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Auth.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<string>>
    {
        private readonly IAuthService authService;
        private readonly ITgLogger tgLogger;

        public LoginCommandHandler(IAuthService authService, ITgLogger tgLogger)
        {
            this.authService = authService;
            this.tgLogger = tgLogger;
        }

        public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserLoginDto.UserName))
            {
                return Result<string>.Failure("Username has not been inputted!");
            }

            var result = await authService.Authenticate(request.UserLoginDto.UserName, request.UserLoginDto.Password, request.HttpContext);

            if (result.IsSuccess)
            {
                await tgLogger.LogInfo($"{request.UserLoginDto.UserName} successfully signed in.");
            }

            return result.IsSuccess ? Result<string>.Success(result.Message) : Result<string>.Failure(result.Message);
        }
    }
}