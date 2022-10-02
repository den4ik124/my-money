using BudgetHistory.API.Controllers;
using BudgetHistory.Application.Auth.Commands;
using BudgetHistory.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BudgetHistory.Api.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseApiController
    {
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

        [HttpPost("register")]
        public async Task<IActionResult> Купшыеук(UserRegistrationDto userRegistrationDto)
        {
            return HandleResult(await Mediator.Send(new RegisterCommand()
            {
                UserRegistrationDto = userRegistrationDto
            }));
        }
    }
}