using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Room;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Rooms.Queries
{
    public class GetRoomByIdQueryHandler : IRequestHandler<GetRoomByIdQuery, Result<IEnumerable<RoomResponseDto>>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public GetRoomByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public Task<Result<IEnumerable<RoomResponseDto>>> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
        {
            var rooms = this.unitOfWork.GetGenericRepository<Room>().GetQuery(r => r.Id.ToString() == request.RoomId && r.Users.Select(u => u.Id.ToString()).Contains(request.UserId));
            if (rooms == null) return Task.FromResult(Result<IEnumerable<RoomResponseDto>>.Failure("Room does not exist or user is not in the room users list."));
            var roomsDto = this.mapper.Map<IEnumerable<RoomResponseDto>>(rooms);
            return Task.FromResult(Result<IEnumerable<RoomResponseDto>>.Success(roomsDto));
        }
    }
}