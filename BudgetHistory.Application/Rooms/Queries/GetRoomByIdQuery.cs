using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Room;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace BudgetHistory.Application.Rooms.Queries
{
    public class GetRoomByIdQuery : IRequest<Result<IEnumerable<RoomResponseDto>>>
    {
        public string UserId { get; set; }
        public string RoomId { get; set; }
        public HttpContext HttpContext { get; set; }
    }
}