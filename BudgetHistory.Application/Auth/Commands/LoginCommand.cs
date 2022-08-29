using MediatR;
using Microsoft.AspNetCore.Http;
using Notebook.Application.Core;
using Notebook.Application.DTOs.Auth;

namespace Notebook.Application.Auth.Commands
{
    public class LoginCommand : IRequest<Result<object>>
    {
        public UserLoginDto UserLoginDto { get; set; }
        public HttpContext HttpContext { get; set; }
        public HttpResponse Response { get; set; }
    }
}