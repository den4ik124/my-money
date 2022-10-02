using BudgetHistory.Application.DTOs.Room;
using BudgetHistory.Application.Rooms.Commands;
using BudgetHistory.Application.Rooms.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BudgetHistory.API.Controllers
{
    public class RoomController : BaseApiController
    {
        [HttpGet()]
        public async Task<IActionResult> GetRoomsList()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userId is null
                ? BadRequest("Unknown user.")
                : HandleResult(await Mediator.Send(new GetRoomsQuery() { UserId = new Guid(userId) }));
        }

        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetRoomById(string roomId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId is null
                ? BadRequest("Unknown user.")
                : HandleResult(await Mediator.Send(new GetRoomByIdQuery() { HttpContext = this.HttpContext, UserId = userId, RoomId = roomId }));
        }

        [HttpPost("create-new-room")]
        public async Task<IActionResult> CreateNewRoom(RoomDto newRoom)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userId is null
                ? BadRequest("Unknown user.")
                : HandleResult(await Mediator.Send(new CreateNewRoomCommand() { NewRoomDto = newRoom, UserId = userId }));
        }

        [HttpPost("room-login")]
        public async Task<IActionResult> LoginRoom(LoginRoomDto loginRoomDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userId is null
                ? BadRequest("Unknown user.")
                : HandleResult(await Mediator.Send(new LoginRoomCommand() { CurrentUserId = userId, HttpContext = this.HttpContext, LoginRoomDto = loginRoomDto }));
        }
    }
}