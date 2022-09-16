using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Room;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BudgetHistory.Application.Rooms.Commands
{
    public class LoginRoomCommand : IRequest<Result<string>>
    {
        public string CurrentUserId { get; set; }
        public LoginRoomDto LoginRoomDto { get; set; }
        public HttpContext HttpContext { get; set; }
        //public HttpResponse Response { get; set; }
    }
}