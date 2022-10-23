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
        private readonly IMapper mapper;
        private readonly INoteService noteService;

        public GetNoteByIdQueryHandler(IMapper mapper, INoteService noteService)
        {
            this.mapper = mapper;
            this.noteService = noteService;
        }

        public async Task<Result<NoteDto>> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var note = await this.noteService.GetNoteById(request.NoteId);

                var response = this.mapper.Map<NoteDto>(note.Value);

                return Result<NoteDto>.Success(response);
            }
            catch (ArgumentNullException ex)
            {
                return Result<NoteDto>.Failure(ex.Message);
            }
        }
    }
}