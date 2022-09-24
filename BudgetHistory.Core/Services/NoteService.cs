using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetHistory.Core.Services
{
    public class NoteService : INoteService
    {
        private readonly IUnitOfWork unitOfWork;

        public NoteService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateNewNote(Note newNote, Currency currency, decimal value, Guid roomId)
        {
            var repo = this.unitOfWork.GetGenericRepository<Note>();

            var lastNote = repo.GetQuery(note => note.RoomId == roomId
                                     && note.Currency == currency)?
                                     .OrderBy(x => x.DateOfCreation).LastOrDefault();

            newNote.Balance = lastNote is null ? value : lastNote.Balance + value;
            newNote.Id = Guid.NewGuid();
            newNote.DateOfCreation = DateTime.UtcNow;
            if (await repo.Add(newNote))
            {
                await this.unitOfWork.CompleteAsync();
                return true;
            }
            return false;
        }

        public bool DeleteNote(Guid noteId)
        {
            throw new NotImplementedException();
        }

        public bool RecalculateBalance(Guid editedNoteId)
        {
            throw new NotImplementedException();
        }

        public bool UpdateNote(Note updatedNote)
        {
            throw new NotImplementedException();
        }
    }
}