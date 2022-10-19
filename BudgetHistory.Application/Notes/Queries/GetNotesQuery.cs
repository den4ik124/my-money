using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Note;
using MediatR;
using System;

namespace BudgetHistory.Application.Notes.Queries
{
    public class GetNotesQuery : QueryBase, IRequest<Result<PagedList<NoteDto>>>
    {
        public Guid RoomId { get; set; }
    }
}