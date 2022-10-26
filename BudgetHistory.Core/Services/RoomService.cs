using BudgetHistory.Core.Extensions;
using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using BudgetHistory.Core.Services.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetHistory.Core.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IEncryptionDecryption encryptionDecryptionService;
        private readonly IConfiguration configuration;
        private readonly ITokenService tokenService;

        //TODO impelement methods here
        public RoomService(IUnitOfWork unitOfWork, IEncryptionDecryption encryptionDecryptionService, IConfiguration configuration, ITokenService tokenService)
        {
            this.unitOfWork = unitOfWork;
            this.encryptionDecryptionService = encryptionDecryptionService;
            this.configuration = configuration;
            this.tokenService = tokenService;
        }

        public Task<ServiceResponse<Room>> GetRoomById<T>(T roomId)
        {
            var room = unitOfWork.GetGenericRepository<Room>().GetQuery(room => room.Id.ToString() == roomId.ToString()).Include(room => room.Users).FirstOrDefault();

            if (room == null)
            {
                return Task.FromResult(new ServiceResponse<Room>() { IsSuccess = false, Message = $"Room \'{roomId}\' does not exist." });
            }

            room.DecryptValues(encryptionDecryptionService, configuration.GetSection(Constants.AppSettings.SecretKey).Value);

            return Task.FromResult(new ServiceResponse<Room>() { IsSuccess = true, Value = room });
        }

        public async Task<ServiceResponse<string>> LogIn(string currentUserId, Guid roomId, string roomPassword)
        {
            var result = await this.GetRoomById(roomId);
            if (!result.IsSuccess)
            {
                var errorMessage = result.Message;
                return new ServiceResponse<string>() { Message = errorMessage, IsSuccess = false };
            }
            var room = result.Value;

            if (!room.Password.Equals(roomPassword))
            {
                var errorMessage = $"Wrong room password!";
                //TODO: Add attempts handler/counter
                return new ServiceResponse<string>() { Message = errorMessage, IsSuccess = false };
            }

            var token = this.tokenService.CreateRoomSessionToken(result.Value, currentUserId);

            return new ServiceResponse<string>() { IsSuccess = true, Value = token, Message = "Successful room login." };
        }

        public async Task<ServiceResponse> CreateRoom(Room newRoom, Guid userId)
        {
            newRoom.Id = Guid.NewGuid();
            newRoom.DateOfCreation = DateTime.UtcNow;
            newRoom.Password = encryptionDecryptionService.Encrypt(newRoom.Password, configuration.GetSection(Constants.AppSettings.SecretKey).Value);

            var userRepository = unitOfWork.GetGenericRepository<User>();

            var user = userRepository.GetQuery(u => u.AssociatedIdentityUserId == userId).Include(r => r.Rooms).FirstOrDefault();
            if (user is null)
            {
                return new ServiceResponse() { IsSuccess = false, Message = "Creation failed. User does not exist." };
            }

            newRoom.OwnerId = user.Id;

            user.Rooms.Append(newRoom);
            var isUserUpdated = userRepository.Update(user);

            newRoom.Users = new List<User>() { user };

            var result = await unitOfWork.GetGenericRepository<Room>().Add(newRoom);
            if (result && await unitOfWork.CompleteAsync())
            {
                return new ServiceResponse() { IsSuccess = true, Message = "Creation succeeded." };
            }
            return new ServiceResponse() { IsSuccess = false, Message = "Creation failed." };
        }
    }
}