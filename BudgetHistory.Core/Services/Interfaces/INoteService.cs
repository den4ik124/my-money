using BudgetHistory.Core.Models;
using System;
using System.Threading.Tasks;

namespace BudgetHistory.Core.Services.Interfaces
{
    public interface INoteService
    {
        Task<bool> CreateNewNote(Note newNote, Currency currency, decimal value, Guid roomId, string roomPassword);

        Task<bool> UpdateNote(Note updatedNote);

        Task<bool> DeleteNote(Guid noteId);
    }
}