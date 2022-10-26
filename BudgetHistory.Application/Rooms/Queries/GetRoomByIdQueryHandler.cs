using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Room;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
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
        private readonly IRoomService roomService;

        public GetRoomByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IRoomService roomService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.roomService = roomService;
        }

        public Task<Result<IEnumerable<RoomResponseDto>>> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
        {
            //TODO разобраться с комнатами
            var room = this.unitOfWork.GetGenericRepository<Room>().GetQuery(r => r.Id.ToString() == request.RoomId && r.Users.Select(u => u.Id.ToString()).Contains(request.UserId));
            if (room == null) return Task.FromResult(Result<IEnumerable<RoomResponseDto>>.Failure("Room does not exist or user is not in the room users list."));
            var roomsDto = this.mapper.Map<IEnumerable<RoomResponseDto>>(room);
            return Task.FromResult(Result<IEnumerable<RoomResponseDto>>.Success(roomsDto));
        }
    }
}