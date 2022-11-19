using AutoMapper;
using BudgetHistory.Abstractions.Interfaces.Data;
using BudgetHistory.Abstractions.Services;
using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Core.Models;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Notes.Queries
{
    public class GetNotesQueryHandler : IRequestHandler<GetNotesQuery, Result<PagedList<NoteDto>>>
    {
        private readonly IMapper _mapper;
        private readonly INoteService _noteService;
        private readonly IGenericRepository<Note> _noteRepository;

        public GetNotesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, INoteService noteService)
        {
            _mapper = mapper;
            _noteService = noteService;
            _noteRepository = unitOfWork.GetGenericRepository<Note>();
        }

        public async Task<Result<PagedList<NoteDto>>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
        {
            var notes = (await _noteService.GetAllNotes(request.RoomId, request.PageParameters.Page, request.PageParameters.Size)).Value;

            var response = _mapper.Map<IEnumerable<NoteDto>>(notes);

            request.PageParameters.Items = await _noteRepository.GetItemsCount();
            return Result<PagedList<NoteDto>>.Success(new PagedList<NoteDto>(response, request.PageParameters));
        }
    }
}