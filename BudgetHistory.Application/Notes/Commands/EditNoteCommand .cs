using MediatR;
using Notebook.Application.Core;
using Notebook.Application.DTOs;

namespace Notebook.Application.Notes.Commands
{
    public class EditNoteCommand : IRequest<Result<string>>
    {
        public NoteDto EditedNote { get; set; }
    }
}