using AutoMapper;
using BudgetHistory.Application.DTOs;
using BudgetHistory.Application.DTOs.Auth;
using BudgetHistory.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace BudgetHistory.API.Mappings
{
    public class ModelsMapper : Profile
    {
        public ModelsMapper()
        {
            CreateMap<Note, NoteDto>();
            CreateMap<NoteDto, Note>();

            CreateMap<Room, RoomDto>();
            CreateMap<RoomDto, Room>();

            CreateMap<UserRegistrationDto, IdentityUser>();
            CreateMap<IdentityUser, UserRegistrationDto>();
            CreateMap<User, IdentityUser>();
            CreateMap<IdentityUser, User>();
        }
    }
}