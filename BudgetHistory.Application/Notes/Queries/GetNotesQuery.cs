using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Note;
using MediatR;

namespace BudgetHistory.Application.Notes.Queries
{
    public class GetNotesQuery : QueryBase, IRequest<Result<PagedList<NoteDto>>>
    {
    }
}