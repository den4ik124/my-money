using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs;
using MediatR;

namespace BudgetHistory.Application.Notes.Commands
{
    public class CreateNoteCommand : IRequest<Result<string>>
    {
        public NoteDto NoteDto { get; set; }
    }
}