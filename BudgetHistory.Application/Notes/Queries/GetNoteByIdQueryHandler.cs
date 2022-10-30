using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Core.Services.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Notes.Queries
{
    public class GetNoteByIdQueryHandler : IRequestHandler<GetNoteByIdQuery, Result<NoteDto>>
    {
        private readonly IMapper _mapper;
        private readonly INoteService _noteService;

        public GetNoteByIdQueryHandler(IMapper mapper, INoteService noteService)
        {
            _mapper = mapper;
            _noteService = noteService;
        }

        public async Task<Result<NoteDto>> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var note = await _noteService.GetNoteById(request.NoteId);

                var response = _mapper.Map<NoteDto>(note.Value);

                return Result<NoteDto>.Success(response);
            }
            catch (ArgumentNullException ex)
            {
                return Result<NoteDto>.Failure(ex.Message);
            }
        }
    }
}