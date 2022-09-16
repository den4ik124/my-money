using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Room;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Rooms.Queries
{
    public class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, Result<IEnumerable<RoomResponseDto>>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public GetRoomsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<Result<IEnumerable<RoomResponseDto>>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
        {
            var rooms = this.unitOfWork.GetGenericRepository<Room>().GetQuery(r => r.Users.Select(u => u.AssociatedIdentityUserId).Contains(request.UserId)).AsEnumerable();
            var roomsDto = mapper.Map<IEnumerable<RoomResponseDto>>(rooms);
            return Result<IEnumerable<RoomResponseDto>>.Success(roomsDto);
        }
    }
}