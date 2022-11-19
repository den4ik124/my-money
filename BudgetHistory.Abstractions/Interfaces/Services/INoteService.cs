using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BudgetHistory.Abstractions.Services
{
    public interface INoteService
    {
        Task<ServiceResponse<Note>> GetNoteById(Guid noteId);

        Task<ServiceResponse<IEnumerable<Note>>> GetAllNotes(Guid roomId, int pageNumber, int pageSize, Expression<Func<Note, bool>> predicate = null,
                                                    Func<IQueryable<Note>, IOrderedQueryable<Note>> orderBy = null);

        Task<ServiceResponse> CreateNewNote(Note newNote, Currency currency, decimal value);

        Task<ServiceResponse> UpdateNote(Note updatedNote);

        Task<ServiceResponse> DeleteNote(Guid noteId);
    }
}