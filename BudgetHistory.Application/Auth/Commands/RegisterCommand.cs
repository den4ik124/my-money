using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Auth;
using MediatR;

namespace BudgetHistory.Application.Auth.Commands
{
    public class RegisterCommand : IRequest<Result<string>>
    {
        public UserRegistrationDto UserRegistrationDto { get; set; }
    }
}