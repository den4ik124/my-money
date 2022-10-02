using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Rooms.Commands
{
    public class LoginRoomCommandHandler : IRequestHandler<LoginRoomCommand, Result<string>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IEncryptionDecryption encryptionDecryptionService;
        private readonly TokenService tokenService;
        private readonly IConfiguration config;
        private readonly UserManager<IdentityUser> userManager;

        public LoginRoomCommandHandler(IUnitOfWork unitOfWork,
                                       IMapper mapper,
                                       IEncryptionDecryption encryptionDecryptionService,
                                       TokenService tokenService,
                                       IConfiguration config,
                                       UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.encryptionDecryptionService = encryptionDecryptionService;
            this.tokenService = tokenService;
            this.config = config;
            this.userManager = userManager;
        }

        public async Task<Result<string>> Handle(LoginRoomCommand request, CancellationToken cancellationToken)
        {
            var room = await this.unitOfWork.GetGenericRepository<Room>().GetById(new Guid(request.LoginRoomDto.RoomId));
            if (room == null)
            {
                var errorMessage = $"Room \'{request.LoginRoomDto.RoomId}\' does not exist.";
                return Result<string>.Failure(errorMessage);
            }
            var encryptedPassword = encryptionDecryptionService.Decrypt(room.Password, config.GetSection("SecretKey").Value);
            if (!encryptedPassword.Equals(request.LoginRoomDto.RoomPassword))
            {
                var errorMessage = $"Wrong room password!";
                //TODO: Add attempts handler/counter
                return Result<string>.Failure(errorMessage);
            }

            var token = this.tokenService.CreateRoomSessionToken(room, request.CurrentUserId);
            request.HttpContext.Response.Cookies.Append(Cookies.RoomAuth, token,
            new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(10),
                SameSite = SameSiteMode.None,
                Secure = true,
            });

            return Result<string>.Success($"Successful login.");
            //return Result<string>.Success($"Successful login.\nRoom token: {token}");
        }
    }
}