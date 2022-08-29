using AutoMapper;
using Notebook.Application.DTOs;
using Notebook.Core;

namespace Notebook.API.Mappings
{
    public class ModelsMapper : Profile
    {
        public ModelsMapper()
        {
            CreateMap<Note, NoteDto>();
            CreateMap<NoteDto, Note>();

            CreateMap<Room, AddressDto>();
            CreateMap<AddressDto, Room>();
        }
    }
}