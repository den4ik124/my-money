using BudgetHistory.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace BudgetHistory.API.Policy.Handlers
{
    public class RoomLoggedInHandler : AuthorizationHandler<RoomLoggedInPolicy>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoomLoggedInPolicy requirement)
        {
            var roomId = context.User.FindFirst(с => с.Type == ClaimConstants.RoomId);
            var userId = context.User.FindFirst(с => с.Type == ClaimConstants.UserId);

            if (roomId.Value == requirement.RoomId && userId.Value == requirement.UserId)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}