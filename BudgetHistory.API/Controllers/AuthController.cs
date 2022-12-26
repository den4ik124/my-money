using BudgetHistory.API.Controllers;
using BudgetHistory.Application.Auth.Commands;
using BudgetHistory.Application.Auth.Queries;
using BudgetHistory.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BudgetHistory.Api.Controllers
{
    public class AuthController : BaseApiController
    {
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IdentityUser>> GetCurrentUser()
        {
            return HandleResult(await Mediator.Send(new GetCurrentUserQuery { UserName = User.FindFirstValue(ClaimTypes.Name) }));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            return HandleResult(await Mediator.Send(new LoginCommand()
            {
                UserLoginDto = userDto,
                HttpContext = base.HttpContext,
            }));
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto userRegistrationDto)
        {
            return HandleResult(await Mediator.Send(new RegisterCommand()
            {
                UserRegistrationDto = userRegistrationDto
            }));
        }
    }
}