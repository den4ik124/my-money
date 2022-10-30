using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Room;
using BudgetHistory.Core.Extensions;
using BudgetHistory.Core.Services.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Rooms.Queries
{
    public class GetRoomByIdQueryHandler : IRequestHandler<GetRoomByIdQuery, Result<RoomResponseDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRoomService _roomService;

        public GetRoomByIdQueryHandler(IMapper mapper, IRoomService roomService)
        {
            _mapper = mapper;
            _roomService = roomService;
        }

        public async Task<Result<RoomResponseDto>> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
        {
            var roomResult = await _roomService.GetRoomById(request.RoomId);

            if (!roomResult.IsSuccess)
            {
                return Result<RoomResponseDto>.Failure(roomResult.Message);
            }

            if (roomResult.Value.IsUserAllowableToReadData(request.UserId))
            {
                return Result<RoomResponseDto>.Failure($"This user (id : {request.UserId}) can't get information from this room. Request access from room owner.");
            }

            var roomsDto = _mapper.Map<RoomResponseDto>(roomResult.Value);
            return Result<RoomResponseDto>.Success(roomsDto);
        }
    }
}