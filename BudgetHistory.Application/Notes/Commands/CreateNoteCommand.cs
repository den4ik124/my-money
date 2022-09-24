using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Note;
using MediatR;

namespace BudgetHistory.Application.Notes.Commands
{
    public class CreateNoteCommand : IRequest<Result<string>>
    {
        public NoteCreationDto NoteDto { get; set; }
    }
}