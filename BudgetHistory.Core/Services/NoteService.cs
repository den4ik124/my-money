using AutoMapper;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetHistory.Core.Services
{
    public class NoteService : INoteService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public NoteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<bool> CreateNewNote(Note newNote, Currency currency, decimal value, Guid roomId)
        {
            var repo = this.unitOfWork.GetGenericRepository<Note>();

            var lastNote = repo.GetQuery(note => note.RoomId == roomId
                                     && note.Currency == currency)?
                                     .OrderBy(x => x.DateOfCreation).LastOrDefault();

            newNote.Balance = lastNote is null ? value : lastNote.Balance + value;
            newNote.Id = Guid.NewGuid();
            if (await repo.Add(newNote))
            {
                await this.unitOfWork.CompleteAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteNote(Guid noteId)
        {
            //await this.unitOfWork.BeginTransactionAsync();
            //var repository = this.unitOfWork.GetGenericRepository<Note>();
            //var noteFromDb = await repository.GetById(noteId);
            //noteFromDb.IsDeleted = true;

            ////TODO: реализовать пересчет баланса при удалении записи

            //if (repository.Update(noteFromDb))
            //{
            //    this.unitOfWork.TransactionCommit();
            //    await this.unitOfWork.CompleteAsync();
            //    return true;
            //}
            //this.unitOfWork.RollbackTransaction();
            return false;
        }

        public async Task<bool> UpdateNote(Note updatedNote)
        {
            //TODO : проверить лучше варианты с пересчетом значений в БД. 1) при изменении значений 2) при одинаковых значениях
            await this.unitOfWork.BeginTransactionAsync();
            var repository = this.unitOfWork.GetGenericRepository<Note>();
            var noteFromDb = await repository.GetById(updatedNote.Id);

            if (noteFromDb.Value == updatedNote.Value)
            {
                updatedNote.DateOfModification = DateTime.UtcNow;

                this.mapper.Map(updatedNote, noteFromDb);
                //TODO здесь должен быть пересчет, если IsDeleted
                repository.Update(noteFromDb);
                this.unitOfWork.TransactionCommit();
                await this.unitOfWork.CompleteAsync();
                return true;
            }
            this.mapper.Map(updatedNote, noteFromDb); //TODO не нравится мне маппинг в этом месте
            var notesToEdit = GetNotesWithUpdatedBalances(updatedNote, repository);

            if (notesToEdit.Count == 0)
            {
                this.unitOfWork.RollbackTransaction();
                return false;
            }
            foreach (var note in notesToEdit)
            {
                repository.Update(note);
            }
            this.unitOfWork.TransactionCommit();
            await this.unitOfWork.CompleteAsync();
            return true;
        }

        private IList<T> GetNotesWithUpdatedBalances<T>(Note updatedNote, IGenericRepository<T> repository) where T : Note
        {
            var orderedNotes = repository.GetQuery(note => note.RoomId == updatedNote.RoomId
                                                       && note.Currency == updatedNote.Currency)
                                         .OrderBy(x => x.DateOfCreation).ToList();
            var noteIndex = orderedNotes.FindIndex(note => note.Id == updatedNote.Id);
            if (noteIndex < 0)
            {
                return new List<T>();
            }
            var notesToEdit = orderedNotes.Skip(noteIndex - 1).ToList();

            if (notesToEdit.Count == 0)
            {
                return notesToEdit;
            }

            var index = 0;

            if (updatedNote.IsDeleted)
            {
                index = noteIndex;
                notesToEdit.RemoveAt(noteIndex);
            }
            else
            {
                index = notesToEdit.FindIndex(note => note.Id == updatedNote.Id);

                notesToEdit[index].Value = updatedNote.Value;
                notesToEdit[index].Balance = notesToEdit[0].Balance + updatedNote.Value;
                notesToEdit[index].DateOfModification = DateTime.UtcNow;
            }

            for (int i = index; i < notesToEdit.Count; i++)
            {
                notesToEdit[i].Balance = notesToEdit[i - 1].Balance + notesToEdit[i].Value;
            }
            return notesToEdit;
        }
    }
}