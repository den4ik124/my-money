using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Responses;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace BudgetHistory.Abstractions.Interfaces.Services
{
    public interface IUserService
    {
        Task<ServiceResponse<User>> GetCurrentUser(IdentityUser identityUser);
    }
}