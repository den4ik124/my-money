using MediatR;
using Notebook.Application.Core;
using Notebook.Application.DTOs;
using System;

namespace Notebook.Application.Notes.Queries
{
    public class GetNoteByIdQuery : IRequest<Result<NoteDto>>
    {
        public Guid NoteId { get; set; }
    }
}