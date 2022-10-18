using BudgetHistory.Core.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BudgetHistory.Core.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateAuthTokenAsync(IdentityUser user);

        string CreateRoomSessionToken(Room room, string userId);

        IEnumerable<Claim> DecodeToken(string authToken);
    }
}