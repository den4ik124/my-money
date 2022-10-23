﻿using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Responses;
using System;
using System.Threading.Tasks;

namespace BudgetHistory.Core.Services.Interfaces
{
    public interface IRoomService
    {
        Task<ServiceResponse<Room>> GetRoomById<T>(T roomId);

        Task<ServiceResponse<string>> LogIn(string currentUserId, Guid roomId, string roomPassword);

        Task<ServiceResponse> CreateRoom(Room newRoom, Guid userId);

        //TODO copy methods
    }
}