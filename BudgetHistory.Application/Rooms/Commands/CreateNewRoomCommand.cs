using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs;
using MediatR;

namespace BudgetHistory.Application.Rooms.Commands
{
    public class CreateNewRoomCommand : IRequest<Result<string>>
    {
        public RoomDto NewRoomDto { get; set; }
        public string UserId { get; set; }
    }
}