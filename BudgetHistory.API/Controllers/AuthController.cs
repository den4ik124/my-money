using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notebook.API.Controllers;
using Notebook.Application.Auth.Commands;
using Notebook.Application.DTOs.Auth;
using System.Threading.Tasks;

namespace Notebook.Api.Controllers
{
    public class AuthController : BaseApiController
    {
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            return HandleResult(await Mediator.Send(new LoginCommand()
            {
                UserLoginDto = userDto,
                HttpContext = base.HttpContext,
                Response = base.Response
            }));
        }
    }
}