using AutoMapper;
using BudgetHistory.Application.DTOs;
using BudgetHistory.Application.DTOs.Auth;
using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Application.DTOs.Room;
using BudgetHistory.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace BudgetHistory.API.Mappings
{
    public class ModelsMapper : Profile
    {
        public ModelsMapper()
        {
            CreateMap<Note, NoteDto>().ReverseMap();

            CreateMap<Room, RoomDto>().ReverseMap();
            CreateMap<Room, RoomResponseDto>().ReverseMap();

            CreateMap<UserRegistrationDto, IdentityUser>().ReverseMap();
            CreateMap<User, IdentityUser>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}