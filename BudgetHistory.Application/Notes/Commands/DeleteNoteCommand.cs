using MediatR;
using Notebook.Application.Core;
using System;

namespace Notebook.Application.Notes.Commands
{
    public class DeleteNoteCommand : IRequest<Result<string>>
    {
        public Guid NoteId { get; set; }
    }
}