using MediatR;
using Notebook.Application.Core;
using Notebook.Application.DTOs;

namespace Notebook.Application.Notes.Queries
{
    public class GetNotesQuery : QueryBase, IRequest<Result<PagedList<NoteDto>>>
    {
    }
}