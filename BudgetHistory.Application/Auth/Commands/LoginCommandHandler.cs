using BudgetHistory.Application.Core;
using BudgetHistory.Auth.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Auth.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<string>>
    {
        private readonly IAuthService authService;

        public LoginCommandHandler(IAuthService authService)
        {
            this.authService = authService;
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