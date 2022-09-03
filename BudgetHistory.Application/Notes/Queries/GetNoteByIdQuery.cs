using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs;
using MediatR;
using System;

namespace BudgetHistory.Application.Notes.Queries
{
    public class GetNoteByIdQuery : IRequest<Result<NoteDto>>
    {
        public Guid NoteId { get; set; }
    }
}