using BudgetHistory.Application.Core;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Rooms.Commands
{
    public class LoginRoomCommandHandler : IRequestHandler<LoginRoomCommand, Result<string>>
    {
        private readonly ITokenService tokenService;
        private readonly IRoomService roomService;

        public LoginRoomCommandHandler(
                                       ITokenService tokenService,
                                       IRoomService roomService)
        {
            this.tokenService = tokenService;
            this.roomService = roomService;
        }

        public async Task<Result<string>> Handle(LoginRoomCommand request, CancellationToken cancellationToken)
        {
            var result = await roomService.LogIn(request.CurrentUserId, new Guid(request.LoginRoomDto.RoomId), request.LoginRoomDto.RoomPassword);

            if (!result.IsSuccess)
            {
                return Result<string>.Failure(result.Message);
            }

            request.HttpContext.Response.Cookies.Append(Cookies.RoomAuth, result.Value,
            new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(10),
                SameSite = SameSiteMode.None,
                Secure = true,
            });

            return Result<string>.Success(result.Message);
        }
    }
}