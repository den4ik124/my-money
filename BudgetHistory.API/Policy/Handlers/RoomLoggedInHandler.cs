using BudgetHistory.Auth.Interfaces;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetHistory.API.Policy.Handlers
{
    public class RoomLoggedInHandler : AuthorizationHandler<RoomLoggedInPolicy>
    {
        private readonly IHttpContextAccessor httpContext;
        private readonly IAuthService authService;
        private readonly IUnitOfWork unitOfWork;

        public RoomLoggedInHandler(IHttpContextAccessor httpContext, IAuthService authService, IUnitOfWork unitOfWork)
        {
            this.httpContext = httpContext;
            this.authService = authService;
            this.unitOfWork = unitOfWork;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoomLoggedInPolicy requirement)
        {
            var token = this.httpContext.HttpContext.Request.Headers[Headers.RoomAuthorization];

            var claims = authService.DecodeToken(token);

            var roomId = claims.FirstOrDefault(c => c.Type == ClaimConstants.RoomId).Value;
            var userId = claims.FirstOrDefault(c => c.Type == ClaimConstants.UserId).Value;

            var roomFromDb = unitOfWork.GetGenericRepository<Room>().GetQuery(room => room.Id == new Guid(roomId)).Include(r => r.Users).FirstOrDefault();
            var userFromDb = unitOfWork.GetGenericRepository<User>().GetQuery(user => user.Id == new Guid(userId)).FirstOrDefault();

            if (roomFromDb.Users.Contains(userFromDb))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}