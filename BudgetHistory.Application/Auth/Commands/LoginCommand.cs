using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BudgetHistory.Application.Auth.Commands
{
    public class LoginCommand : IRequest<Result<UserDataDto>>
    {
        public UserLoginDto UserLoginDto { get; set; }
        public HttpContext HttpContext { get; set; }
    }
}