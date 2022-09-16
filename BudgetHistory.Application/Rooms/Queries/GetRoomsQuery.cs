using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Room;
using MediatR;
using System;
using System.Collections.Generic;

namespace BudgetHistory.Application.Rooms.Queries
{
    public class GetRoomsQuery : IRequest<Result<IEnumerable<RoomResponseDto>>>
    {
        public Guid UserId { get; set; }
    }
}