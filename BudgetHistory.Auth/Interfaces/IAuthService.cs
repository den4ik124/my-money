using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BudgetHistory.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> Authenticate(string userName, string password, HttpContext context);
    }
}