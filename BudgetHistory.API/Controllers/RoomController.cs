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
            if (userId is null)
            {
                return BadRequest("Unknown user.");
            }
            return HandleResult(await Mediator.Send(new GetRoomsQuery() { UserId = new Guid(userId) }));
            //return new OkObjectResult("Rooms list here");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            return new OkObjectResult($"UserId: {userId}");
        }

        [HttpPost("create-new-room")]
        public async Task<IActionResult> CreateNewRoom(RoomDto newRoom)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return HandleResult(await Mediator.Send(new CreateNewRoomCommand() { NewRoomDto = newRoom, UserId = userId }));
        }
    }
}