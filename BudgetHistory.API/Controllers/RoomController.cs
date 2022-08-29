using Microsoft.AspNetCore.Mvc;
using Notebook.API.Controllers;
using System;
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
    }
}