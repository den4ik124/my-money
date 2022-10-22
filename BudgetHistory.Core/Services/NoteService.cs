using AutoMapper;
using BudgetHistory.Core.Extensions;
using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;
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
        private readonly IEncryptionDecryption encryptionDecryptionService;
        private readonly IConfiguration configuration;
        private readonly IGenericRepository<Note> noteRepository;
        private readonly IGenericRepository<Room> roomRepository;

        public NoteService(IUnitOfWork unitOfWork, IMapper mapper, IEncryptionDecryption encryptionDecryption, IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.encryptionDecryptionService = encryptionDecryption;
            this.configuration = configuration;
            this.noteRepository = unitOfWork.GetGenericRepository<Note>();
            this.roomRepository = unitOfWork.GetGenericRepository<Room>();
        }

        public async Task<bool> CreateNewNote(Note newNote, Currency currency, decimal value, Guid roomId, string roomPassword)
        {
            var lastNote = noteRepository.GetQuery(note => note.RoomId == roomId
                                     && note.Currency == currency)?
                                     .OrderBy(x => x.DateOfCreation).LastOrDefault();
            if (lastNote is not null)
            {
                lastNote.DecryptValues(encryptionDecryptionService, roomPassword);
                newNote.Balance = lastNote.Balance + value;
            }
            else
            {
                newNote.Balance = value;
            }

            newNote.Id = Guid.NewGuid();
            newNote.EncryptValues(encryptionDecryptionService, roomPassword);

            if (await noteRepository.Add(newNote))
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
            //TODO : реализовать update с учетом шифрованя данных
            await this.unitOfWork.BeginTransactionAsync();

            var oldNote = await noteRepository.GetById(updatedNote.Id);
            if (oldNote is null)
            {
                return false;
            }

            var room = await roomRepository.GetById(updatedNote.RoomId);
            var decryptedPassword = encryptionDecryptionService.Decrypt(room.Password, configuration.GetSection(Constants.AppSettings.SecretKey).Value);

            oldNote.DecryptValues(encryptionDecryptionService, decryptedPassword);
            updatedNote.EncryptValues(encryptionDecryptionService, decryptedPassword);

            updatedNote.DateOfModification = DateTime.UtcNow;

            if (oldNote.Value == updatedNote.Value
                && oldNote.IsDeleted == updatedNote.IsDeleted
                && oldNote.Currency == updatedNote.Currency)
            {
                this.mapper.Map(updatedNote, oldNote);
                noteRepository.Update(oldNote);
                this.unitOfWork.TransactionCommit();
                await this.unitOfWork.CompleteAsync();
                return true;
            }

            var notesToEdit = GetNotesWithUpdatedBalances(oldNote, updatedNote, decryptedPassword);

            if (!notesToEdit.Any())
            {
                this.unitOfWork.RollbackTransaction();
                return false;
            }

            foreach (var note in notesToEdit)
            {
                noteRepository.Update(note);
            }
            this.unitOfWork.TransactionCommit();
            await this.unitOfWork.CompleteAsync();
            return true;
        }

        private IEnumerable<Note> GetNotesWithUpdatedBalances(Note oldNote, Note updatedNote, string roomPassword)
        {
            if (oldNote.Currency == updatedNote.Currency)
            {
                this.mapper.Map(updatedNote, oldNote);
                var notesToUpdate = noteRepository.GetQuery(note => note.RoomId == updatedNote.RoomId && note.DateOfCreation >= oldNote.DateOfCreation && note.Currency == oldNote.Currency).ToList();

                foreach (var item in notesToUpdate)
                {
                    item.DecryptValues(encryptionDecryptionService, roomPassword);
                }

                var groupInitialBalance = oldNote.Balance - oldNote.Value < 0 ? 0 : oldNote.Balance - oldNote.Value;

                return RecalculateBalances(notesToUpdate, groupInitialBalance, roomPassword);
            }

            var currencyGroups = noteRepository.GetQuery(note => note.RoomId == updatedNote.RoomId
                                                              && note.DateOfCreation >= oldNote.DateOfCreation
                                                              && !note.IsDeleted)
                                               .AsEnumerable()
                                               .GroupBy(note => note.Currency);

            foreach (var currencyGroup in currencyGroups)
            {
                foreach (var item in currencyGroup)
                {
                    item.DecryptValues(encryptionDecryptionService, roomPassword);
                }
            }

            var oldCurrencyGroup = currencyGroups.FirstOrDefault(group => group.Key == oldNote.Currency)
                                                 .OrderBy(x => x.DateOfCreation)
                                                 .ToList();

            updatedNote.DateOfCreation = oldNote.DateOfCreation;

            var newCurrencyGroup = new List<Note>();
            var newGroupInitialBalance = 0m;

            if (currencyGroups.Any(group => group.Key == updatedNote.Currency))
            {
                newCurrencyGroup = currencyGroups.FirstOrDefault(group => group.Key == updatedNote.Currency)
                                                 .OrderBy(x => x.DateOfCreation)
                                                 .ToList();
                var newGroupFirstElement = newCurrencyGroup.FirstOrDefault(); //получили первый элемент новой коллекции
                newGroupInitialBalance = newGroupFirstElement.Balance - newGroupFirstElement.Value; //запомнили -1 баланс
            }

            this.mapper.Map(updatedNote, oldNote);

            newCurrencyGroup.Insert(0, oldNote);

            //TODO : Пересчитать баланс новых записей
            newCurrencyGroup = RecalculateBalances(newCurrencyGroup, newGroupInitialBalance, roomPassword);

            var oldGroupFirstElement = oldCurrencyGroup.FirstOrDefault(); //получили первый элемент новой коллекции
            var oldGroupInitialBalance = oldGroupFirstElement is null ? 0 : oldGroupFirstElement.Balance - oldGroupFirstElement.Value; //запомнили -1 баланс

            oldCurrencyGroup.Remove(oldNote);

            //TODO : Пересчитать баланс старых записей
            oldCurrencyGroup = RecalculateBalances(oldCurrencyGroup.ToList(), oldGroupInitialBalance, roomPassword);

            var concatedList = oldCurrencyGroup.Concat(newCurrencyGroup);

            return concatedList;
        }

        private List<Note> RecalculateBalances(List<Note> notes, decimal initialBalance, string roomPassword)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                if (i == 0)
                {
                    notes[i].Balance = initialBalance + notes[i].Value;
                }
                else
                {
                    notes[i].Balance = notes[i - 1].Balance + notes[i].Value;
                }
                notes[i].EncryptValues(encryptionDecryptionService, roomPassword);
            }

            return notes;
        }
    }
}