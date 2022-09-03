using BudgetHistory.Application.Core;
using MediatR;
using System;

namespace BudgetHistory.Application.Notes.Commands
{
    public class DeleteNoteCommand : IRequest<Result<string>>
    {
        public Guid NoteId { get; set; }
    }
}