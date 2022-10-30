using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Rooms.Commands
{
    public class CreateNewRoomCommandHandler : IRequestHandler<CreateNewRoomCommand, Result<string>>
    {
        private readonly IMapper _mapper;
        private readonly IRoomService _roomService;

        public CreateNewRoomCommandHandler(IMapper mapper, IRoomService roomService)
        {
            _mapper = mapper;
            _roomService = roomService;
        }

        public async Task<Result<string>> Handle(CreateNewRoomCommand request, CancellationToken cancellationToken)
        {
            var room = _mapper.Map<Room>(request.NewRoomDto);

            var result = await _roomService.CreateRoom(room, new Guid(request.UserId));
            return result.IsSuccess ? Result<string>.Success(result.Message) : Result<string>.Failure(result.Message);
        }
    }
}