using AutoMapper;
using BudgetHistory.Abstractions.Interfaces.Data;
using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Room;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetRoomsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public Task<Result<IEnumerable<RoomResponseDto>>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
        {
            var rooms = _unitOfWork.GetGenericRepository<Room>().GetQuery(r => r.Users.Select(u => u.AssociatedIdentityUserId).Contains(request.UserId)).AsEnumerable();
            var roomsDto = _mapper.Map<IEnumerable<RoomResponseDto>>(rooms);
            return Task.FromResult(Result<IEnumerable<RoomResponseDto>>.Success(roomsDto));
        }
    }
}