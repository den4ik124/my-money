using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
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
        private readonly IHttpContextAccessor _httpContext;
        private readonly ITokenService _jwtTokenService;
        private readonly IUnitOfWork _unitOfWork;

        public RoomLoggedInHandler(IHttpContextAccessor httpContext, ITokenService jwtTokenService, IUnitOfWork unitOfWork)
        {
            _httpContext = httpContext;
            _jwtTokenService = jwtTokenService;
            _unitOfWork = unitOfWork;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoomLoggedInPolicy requirement)
        {
            var token = _httpContext.HttpContext.Request.Headers[Headers.RoomAuthorization];

            if (!token.Any())
                return Task.CompletedTask;

            var claims = _jwtTokenService.DecodeToken(token);

            var roomId = claims.FirstOrDefault(c => c.Type == ClaimConstants.RoomId).Value;
            var userId = claims.FirstOrDefault(c => c.Type == ClaimConstants.UserId).Value;

            var roomFromDb = _unitOfWork.GetGenericRepository<Room>().GetQuery(room => room.Id == new Guid(roomId)).Include(r => r.Users).FirstOrDefault()
                ?? throw new ArgumentNullException($"Room Id: {roomId}", $"Room does not exist.");
            var userFromDb = _unitOfWork.GetGenericRepository<User>().GetQuery(user => user.AssociatedIdentityUserId == new Guid(userId)).FirstOrDefault()
                ?? throw new ArgumentNullException($"User Id: {userId}", $"User does not exist.");

            if (roomFromDb.Users.Contains(userFromDb))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}