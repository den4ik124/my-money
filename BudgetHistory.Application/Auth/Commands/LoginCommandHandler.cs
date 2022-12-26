using AutoMapper;
using BudgetHistory.Abstractions.Interfaces.Data;
using BudgetHistory.Abstractions.Interfaces.Services;
using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Auth;
using BudgetHistory.Auth.Interfaces;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Resources;
using BudgetHistory.Logging;
using BudgetHistory.Logging.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Auth.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<UserDataDto>>
    {
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly CustomLogger _logger;

        public LoginCommandHandler(IAuthService authService, ICustomLoggerFactory loggerFactory, IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
        {
            _authService = authService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _logger = loggerFactory.CreateLogger<LoginCommandHandler>();
        }

        public async Task<Result<UserDataDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserLoginDto.UserName))
            {
                return Result<UserDataDto>.Failure(ResponseMessages.EmptyUserName);
            }

            var authResult = await _authService.Authenticate(request.UserLoginDto.UserName, request.UserLoginDto.Password, request.HttpContext);

            var userResult = await _userService.GetCurrentUser(authResult.Value);
            if (authResult.IsSuccess && userResult.IsSuccess)
            {
                var userResponseDto = _mapper.Map<User, UserDataDto>(userResult.Value);
                await _logger.LogInfo(string.Format(ResponseMessages.UserSuccessfullLogin, request.UserLoginDto.UserName));
                return Result<UserDataDto>.Success(userResponseDto);
            }

            return Result<UserDataDto>.Failure(authResult.Message);
        }
    }
}