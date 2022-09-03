using AutoMapper;
using BudgetHistory.Application.DTOs;
using BudgetHistory.Core.Models;

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
        }
    }
}