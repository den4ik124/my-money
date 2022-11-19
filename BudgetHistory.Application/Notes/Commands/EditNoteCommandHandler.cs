using AutoMapper;
using BudgetHistory.Abstractions.Services;
using BudgetHistory.Application.Core;
using BudgetHistory.Core.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Notes.Commands
{
    public class EditNoteCommandHandler : IRequestHandler<EditNoteCommand, Result<string>>
    {
        private readonly INoteService _noteService;
        private readonly IMapper _mapper;

        public EditNoteCommandHandler(INoteService noteService, IMapper mapper)
        {
            _noteService = noteService;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(EditNoteCommand request, CancellationToken cancellationToken)
        {
            var note = _mapper.Map<Note>(request.EditedNote);

            var result = await _noteService.UpdateNote(note);

            return result.IsSuccess ? Result<string>.Success(result.Message) : Result<string>.Failure(result.Message);
        }
    }
}