using BudgetHistory.Application.DTOs;
using BudgetHistory.Application.Rooms.Commands;
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
            return new OkObjectResult("Rooms list here");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(Guid id)
        {
            return new OkObjectResult(id);
        }

        [HttpPost("create-new-room")]
        public async Task<IActionResult> CreateNewRoom(RoomDto newRoom)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return HandleResult(await Mediator.Send(new CreateNewRoomCommand() { NewRoomDto = newRoom, UserId = userId }));
        }
    }
}