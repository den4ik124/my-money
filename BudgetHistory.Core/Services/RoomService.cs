using BudgetHistory.Core.Extensions;
using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using BudgetHistory.Core.Services.Responses;
using BudgetHistory.Logging;
using BudgetHistory.Logging.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetHistory.Core.Services
{
    public class RoomService : BaseService, IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionDecryption _encryptionDecryptionService;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly CustomLogger _logger;

        //TODO impelement methods here
        public RoomService(IUnitOfWork unitOfWork, IEncryptionDecryption encryptionDecryptionService, IConfiguration configuration, ITokenService tokenService, ICustomLoggerFactory loggerFactory)
        {
            _unitOfWork = unitOfWork;
            _encryptionDecryptionService = encryptionDecryptionService;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = loggerFactory.CreateLogger<RoomService>();
        }

        public async Task<ServiceResponse<Room>> GetRoomById<T>(T roomId)
        {
            return await Task.Run(async () =>
            {
                var room = _unitOfWork.GetGenericRepository<Room>()
                                     .GetQuery(room => room.Id.ToString() == roomId.ToString())
                                     .Include(room => room.Users)
                                     .FirstOrDefault();

                if (room == null)
                {
                    return await base.Failed<Room>(_logger, $"Room \'{roomId}\' does not exist.");
                }

                await room.DecryptValues(_encryptionDecryptionService, _configuration.GetSection(Constants.AppSettings.SecretKey).Value);
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
                //TODO: Add attempts handler/counter
                return await base.Failed<string>(_logger, "Wrong room password!");
            }

            var token = _tokenService.CreateRoomSessionToken(result.Value, currentUserId);

            return ServiceResponse<string>.Success(token);
        }

        public async Task<ServiceResponse> CreateRoom(Room newRoom, Guid userId)
        {
            newRoom.Id = Guid.NewGuid();
            newRoom.EncryptedPassword = await _encryptionDecryptionService.Encrypt(newRoom.Password, _configuration.GetSection(Constants.AppSettings.SecretKey).Value);

            var userRepository = _unitOfWork.GetGenericRepository<User>();

            var user = userRepository.GetQuery(u => u.AssociatedIdentityUserId == userId).Include(r => r.Rooms).FirstOrDefault();
            if (user is null)
            {
                return await base.Failed<string>(_logger, "Creation failed. User does not exist");
            }

            newRoom.OwnerId = user.Id;

            _ = user.Rooms.Append(newRoom);
            var isUserUpdated = userRepository.Update(user);

            newRoom.Users = new List<User>() { user };

            var result = await _unitOfWork.GetGenericRepository<Room>().Add(newRoom);

            return result && await _unitOfWork.CompleteAsync() ? ServiceResponse.Success("Creation succeeded.") : await base.Failed<string>(_logger, "Creation failed.");
        }

        public async Task<ServiceResponse<IEnumerable<Room>>> GetRoomsForUser(Guid userId)
            => await Task.Run(() =>
            {
                var rooms = _unitOfWork.GetGenericRepository<Room>().GetQuery(r => r.Users.Select(u => u.AssociatedIdentityUserId).Contains(userId));

                return rooms.Any() ? ServiceResponse<IEnumerable<Room>>.Success(rooms) : ServiceResponse<IEnumerable<Room>>.Failure($"There are no valid rooms for a particular user yet");
            });
    }
}