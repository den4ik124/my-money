using MediatR;
using Notebook.Application.Core;
using Notebook.Application.DTOs;

namespace Notebook.Application.Notes.Commands
{
    public class CreateNoteCommand : IRequest<Result<string>>
    {
        public NoteDto NoteDto { get; set; }
    }
}