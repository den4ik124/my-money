using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Room;
using MediatR;

namespace BudgetHistory.Application.Rooms.Queries
{
    public class GetRoomByIdQuery : IRequest<Result<RoomResponseDto>>
    {
        public string UserId { get; set; }
        public string RoomId { get; set; }
    }
}