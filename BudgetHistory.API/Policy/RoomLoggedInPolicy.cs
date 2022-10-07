using Microsoft.AspNetCore.Authorization;

namespace BudgetHistory.API.Policy
{
    public class RoomLoggedInPolicy : IAuthorizationRequirement
    {
        public string RoomId { get; }
        public string UserId { get; }
    }
}