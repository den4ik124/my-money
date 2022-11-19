using BudgetHistory.Abstractions.Services;
using BudgetHistory.Application.Core;
using BudgetHistory.Core.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Rooms.Commands
{
    public class LoginRoomCommandHandler : IRequestHandler<LoginRoomCommand, Result<string>>
    {
        private readonly IRoomService _roomService;

        public LoginRoomCommandHandler(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task<Result<string>> Handle(LoginRoomCommand request, CancellationToken cancellationToken)
        {
            var result = await _roomService.LogIn(request.CurrentUserId, new Guid(request.LoginRoomDto.RoomId), request.LoginRoomDto.RoomPassword);

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