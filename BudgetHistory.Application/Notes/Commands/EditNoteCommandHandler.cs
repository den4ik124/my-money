using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Notes.Commands
{
    public class EditNoteCommandHandler : IRequestHandler<EditNoteCommand, Result<string>>
    {
        private readonly INoteService noteService;
        private readonly IMapper mapper;

        public EditNoteCommandHandler(INoteService noteService, IMapper mapper)
        {
            this.noteService = noteService;
            this.mapper = mapper;
        }

        public async Task<Result<string>> Handle(EditNoteCommand request, CancellationToken cancellationToken)
        {
            var note = this.mapper.Map<Note>(request.EditedNote);

            var result = await noteService.UpdateNote(note);

            return result.IsSuccess ? Result<string>.Success(result.Message) : Result<string>.Failure(result.Message);
        }
    }
}