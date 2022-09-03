using BudgetHistory.API.Controllers;
using BudgetHistory.Application.Auth.Commands;
using BudgetHistory.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BudgetHistory.Api.Controllers
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