using AutoMapper;
using BudgetHistory.Application.DTOs.Auth;
using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Application.DTOs.Room;
using BudgetHistory.Core.Models;
using Microsoft.AspNetCore.Identity;
using System;

namespace BudgetHistory.API.Mappings
{
    public class ModelsMapper : Profile
    {
        public ModelsMapper()
        {
            CreateMap<Note, Note>();
            CreateMap<Note, NoteDto>()
                .ForMember(d => d.Currency, op => op.MapFrom(src => src.Currency.ToString()));
            CreateMap<NoteDto, Note>()
                .ForMember(d => d.Currency, op => op.MapFrom(src => Enum.Parse(typeof(Currency), src.Currency.ToUpper())));
            CreateMap<NoteCreationDto, Note>()
                .ForMember(d => d.Currency, op => op.MapFrom(src => Enum.Parse(typeof(Currency), src.Currency.ToUpper())));

            CreateMap<Room, RoomDto>().ReverseMap();
            CreateMap<Room, RoomResponseDto>().ReverseMap();

            CreateMap<UserRegistrationDto, IdentityUser>().ReverseMap();
            CreateMap<User, IdentityUser>().ReverseMap();
            CreateMap<User, UserDataDto>().ReverseMap();
        }
    }
}