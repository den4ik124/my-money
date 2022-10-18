using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace BudgetHistory.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> Authenticate(string userName, string password, HttpContext context);

        Task<AuthResult> RegisterUser(IdentityUser identityUser, string password);
    }
}