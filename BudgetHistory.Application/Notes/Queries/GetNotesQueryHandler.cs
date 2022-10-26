using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Notes.Queries
{
    public class GetNotesQueryHandler : IRequestHandler<GetNotesQuery, Result<PagedList<NoteDto>>>
    {
        private readonly IMapper mapper;
        private readonly INoteService noteService;
        private readonly IGenericRepository<Note> noteRepository;

        public GetNotesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, INoteService noteService)
        {
            this.mapper = mapper;
            this.noteService = noteService;
            this.noteRepository = unitOfWork.GetGenericRepository<Note>();
        }

        public async Task<Result<PagedList<NoteDto>>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
        {
            var notes = await noteService.GetAllNotes(request.RoomId, request.PageParameters.Page, request.PageParameters.Size);

            var response = this.mapper.Map<IEnumerable<NoteDto>>(notes);

            request.PageParameters.Items = await this.noteRepository.GetItemsCount();
            return Result<PagedList<NoteDto>>.Success(new PagedList<NoteDto>(response, request.PageParameters));
        }
    }
}