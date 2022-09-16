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
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public RegisterCommandHandler(IAuthService authService, IMapper mapper)
        {
            this.authService = authService;
            this.mapper = mapper;
        }

        public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var identityUser = mapper.Map<IdentityUser>(request.UserRegistrationDto);
            var result = await authService.RegisterUser(identityUser, request.UserRegistrationDto.Password);
            return result.IsSuccess ? Result<string>.Success(result.Message) : Result<string>.Failure(result.Message);
        }
    }
}