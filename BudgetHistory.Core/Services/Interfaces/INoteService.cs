using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BudgetHistory.Core.Services.Interfaces
{
    public interface INoteService
    {
        Task<IEnumerable<Note>> GetAllNotes(Guid roomId, int pageNumber, int pageSize, Expression<Func<Note, bool>> predicate = null,
                                                    Func<IQueryable<Note>, IOrderedQueryable<Note>> orderBy = null);

        Task<NoteServiceResponse> CreateNewNote(Note newNote, Currency currency, decimal value, Guid roomId, string roomPassword);

        Task<NoteServiceResponse> UpdateNote(Note updatedNote);

        Task<NoteServiceResponse> DeleteNote(Guid noteId);
    }
}