using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Auth;
using MediatR;

namespace BudgetHistory.Application.Auth.Queries
{
    public class GetCurrentUserQuery : IRequest<Result<UserDataDto>>
    {
        public string UserName { get; set; }
    }
}