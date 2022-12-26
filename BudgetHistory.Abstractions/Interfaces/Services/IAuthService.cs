using BudgetHistory.Core.Services.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BudgetHistory.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse<IdentityUser>> GetCurrentUser(string userName);

        Task<ServiceResponse<IList<string>>> GetUserRoles(IdentityUser user);

        Task<ServiceResponse<IdentityUser>> Authenticate(string userName, string password, HttpContext context);

        Task<ServiceResponse> RegisterUser(IdentityUser identityUser, string password);
    }
}