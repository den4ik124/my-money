using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Auth.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Auth.Commands
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public RegisterCommandHandler(IAuthService authService, IMapper mapper)
        {
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var identityUser = _mapper.Map<IdentityUser>(request.UserRegistrationDto);
            var result = await _authService.RegisterUser(identityUser, request.UserRegistrationDto.Password);
            return result.IsSuccess ? Result<string>.Success(result.Message) : Result<string>.Failure(result.Message);
        }
    }
}