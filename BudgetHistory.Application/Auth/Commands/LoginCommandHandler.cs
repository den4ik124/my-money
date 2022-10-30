using BudgetHistory.Application.Core;
using BudgetHistory.Auth.Interfaces;
using BudgetHistory.Logging;
using BudgetHistory.Logging.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Auth.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<string>>
    {
        private readonly IAuthService _authService;
        private readonly CustomLogger _logger;

        public LoginCommandHandler(IAuthService authService, ICustomLoggerFactory loggerFactory)
        {
            _authService = authService;
            _logger = loggerFactory.CreateLogger<LoginCommandHandler>();
        }

        public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserLoginDto.UserName))
            {
                return Result<string>.Failure("Username has not been inputted!");
            }

            var result = await _authService.Authenticate(request.UserLoginDto.UserName, request.UserLoginDto.Password, request.HttpContext);

            if (result.IsSuccess)
            {
                await _logger.LogInfo($"{request.UserLoginDto.UserName} successfully signed in.");
            }

            return result.IsSuccess ? Result<string>.Success(result.Message) : Result<string>.Failure(result.Message);
        }
    }
}