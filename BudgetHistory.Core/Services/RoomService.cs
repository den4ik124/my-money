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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionDecryption _encryptionDecryptionService;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        //TODO impelement methods here
        public RoomService(IUnitOfWork unitOfWork, IEncryptionDecryption encryptionDecryptionService, IConfiguration configuration, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _encryptionDecryptionService = encryptionDecryptionService;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        public async Task<ServiceResponse<Room>> GetRoomById<T>(T roomId)
        {
            return await Task.Run(() =>
            {
                var room = _unitOfWork.GetGenericRepository<Room>()
                                     .GetQuery(room => room.Id.ToString() == roomId.ToString())
                                     .Include(room => room.Users)
                                     .FirstOrDefault();

                if (room == null)
                {
                    return ServiceResponse<Room>.Failure($"Room \'{roomId}\' does not exist.");
                }

                room.DecryptValues(_encryptionDecryptionService, _configuration.GetSection(Constants.AppSettings.SecretKey).Value);
                return ServiceResponse<Room>.Success(room);
            });
        }

        public async Task<ServiceResponse<string>> LogIn(string currentUserId, Guid roomId, string roomPassword)
        {
            var result = await GetRoomById(roomId);
            if (!result.IsSuccess)
            {
                var errorMessage = result.Message;
                return ServiceResponse<string>.Failure(errorMessage);
            }
            var room = result.Value;

            if (!room.Password.Equals(roomPassword))
            {
                var errorMessage = $"Wrong room password!";
                //TODO: Add attempts handler/counter
                return ServiceResponse<string>.Failure(errorMessage);
            }

            var token = _tokenService.CreateRoomSessionToken(result.Value, currentUserId);

            return ServiceResponse<string>.Success(token);
        }

        public async Task<ServiceResponse> CreateRoom(Room newRoom, Guid userId)
        {
            newRoom.Id = Guid.NewGuid();
            newRoom.EncryptedPassword = _encryptionDecryptionService.Encrypt(newRoom.Password, _configuration.GetSection(Constants.AppSettings.SecretKey).Value);

            var userRepository = _unitOfWork.GetGenericRepository<User>();

            var user = userRepository.GetQuery(u => u.AssociatedIdentityUserId == userId).Include(r => r.Rooms).FirstOrDefault();
            if (user is null)
            {
                return ServiceResponse.Failure("Creation failed. User does not exist.");
            }

            newRoom.OwnerId = user.Id;

            _ = user.Rooms.Append(newRoom);
            var isUserUpdated = userRepository.Update(user);

            newRoom.Users = new List<User>() { user };

            var result = await _unitOfWork.GetGenericRepository<Room>().Add(newRoom);

            return result && await _unitOfWork.CompleteAsync() ? ServiceResponse.Success("Creation succeeded.") : ServiceResponse.Failure("Creation failed.");
        }

        public async Task<ServiceResponse<IEnumerable<Room>>> GetRoomsForUser(Guid userId)
            => await Task.Run(() =>
            {
                var rooms = _unitOfWork.GetGenericRepository<Room>().GetQuery(r => r.Users.Select(u => u.AssociatedIdentityUserId).Contains(userId));

                return rooms.Any() ? ServiceResponse<IEnumerable<Room>>.Success(rooms) : ServiceResponse<IEnumerable<Room>>.Failure($"There are no valid rooms for a particular user yet");
            });
    }
}