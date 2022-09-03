using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace BudgetHistory.Application.Rooms.Queries
{
    public class GetRoomsQuery : IRequest<Result<IEnumerable<RoomDto>>>
    {
        public Guid UserId { get; set; }
    }
}