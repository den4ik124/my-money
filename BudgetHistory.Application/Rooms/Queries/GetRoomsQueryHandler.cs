using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Rooms.Queries
{
    public class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, Result<IEnumerable<RoomDto>>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public GetRoomsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<Result<IEnumerable<RoomDto>>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
        {
            var rooms = this.unitOfWork.GetGenericRepository<Room>().GetQuery(r => r.Users.Select(u => u.Id).Contains(request.UserId));
            var roomsDto = mapper.Map<IEnumerable<RoomDto>>(rooms);
            return Result<IEnumerable<RoomDto>>.Success(roomsDto);
        }
    }
}