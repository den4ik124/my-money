using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Rooms.Commands
{
    public class CreateNewRoomCommandHandler : IRequestHandler<CreateNewRoomCommand, Result<string>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IEncryptionDecryption encryptionDecryptionService;
        private readonly IConfiguration config;

        public CreateNewRoomCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IEncryptionDecryption encryptionDecryptionService, IConfiguration config)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.encryptionDecryptionService = encryptionDecryptionService;
            this.config = config;
        }

        public async Task<Result<string>> Handle(CreateNewRoomCommand request, CancellationToken cancellationToken)
        {
            request.NewRoomDto.Id = Guid.NewGuid();
            request.NewRoomDto.DateOfCreation = DateTime.UtcNow;
            request.NewRoomDto.Password = encryptionDecryptionService.Encrypt(request.NewRoomDto.Password, config.GetSection("SecretKey").Value);

            var room = mapper.Map<Room>(request.NewRoomDto);

            var userRepository = unitOfWork.GetGenericRepository<User>();
            var user = userRepository.GetQuery(u => u.AssociatedIdentityUserId.ToString() == request.UserId).Include(r => r.Rooms).FirstOrDefault();
            if (user is null)
            {
                return Result<string>.Failure("Creation failed. User does not exist.");
            }
            request.NewRoomDto.OwnerId = user.Id;

            user.Rooms.Append(room);
            var isUserUpdated = userRepository.Update(user);

            room.Users = new List<User>() { user };

            var result = await unitOfWork.GetGenericRepository<Room>().Add(room);
            if (result && await unitOfWork.CompleteAsync())
            {
                return Result<string>.Success("Creation succeeded.");
            }
            return Result<string>.Failure("Creation failed.");
        }
    }
}