using BudgetHistory.Core.Services.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace BudgetHistory.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse> Authenticate(string userName, string password, HttpContext context);

        Task<ServiceResponse> RegisterUser(IdentityUser identityUser, string password);
    }
}