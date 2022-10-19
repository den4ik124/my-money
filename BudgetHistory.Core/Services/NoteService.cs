using AutoMapper;
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
                lastNote.Value = decimal.Parse(encryptionDecryptionService.Decrypt(lastNote.EncryptedValue.ToString(), roomPassword));
                lastNote.Balance = decimal.Parse(encryptionDecryptionService.Decrypt(lastNote.EncryptedBalance.ToString(), roomPassword));
                newNote.Balance = lastNote.Balance + value;
            }
            else
            {
                newNote.Balance = value;
            }

            newNote.Id = Guid.NewGuid();

            newNote.EncryptedValue = encryptionDecryptionService.Encrypt(newNote.Value.ToString(), roomPassword);
            newNote.EncryptedBalance = encryptionDecryptionService.Encrypt(newNote.Balance.ToString(), roomPassword);

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

            oldNote.Value = decimal.Parse(encryptionDecryptionService.Decrypt(oldNote.EncryptedValue, decryptedPassword));
            updatedNote.EncryptedValue = encryptionDecryptionService.Encrypt(updatedNote.Value.ToString(), decryptedPassword);
            updatedNote.EncryptedBalance = encryptionDecryptionService.Encrypt(oldNote.Balance.ToString(), decryptedPassword);
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

            this.mapper.Map(updatedNote, oldNote);

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
                var currencyGroup = noteRepository.GetQuery(note => note.RoomId == updatedNote.RoomId).GroupBy(note => note.Currency).FirstOrDefault(group => group.Key == oldNote.Currency).OrderBy(x => x.DateOfCreation).ToList();
                var noteIndex = currencyGroup.FindIndex(note => note.Id == updatedNote.Id);
                if (noteIndex < 0)
                {
                    return new List<Note>();
                }
                var notesToEdit = currencyGroup.Skip(noteIndex - 1).ToList();

                foreach (var note in notesToEdit)
                {
                    note.Value = decimal.Parse(encryptionDecryptionService.Decrypt(note.EncryptedValue, roomPassword));
                    note.Balance = decimal.Parse(encryptionDecryptionService.Decrypt(note.EncryptedBalance, roomPassword));
                }

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
                    if (i == 0)
                    {
                        notesToEdit[i].Balance = notesToEdit[i].Value;
                        continue;
                    }
                    notesToEdit[i].Balance = notesToEdit[i - 1].Balance + notesToEdit[i].Value;
                }

                foreach (var note in notesToEdit)
                {
                    note.EncryptedValue = encryptionDecryptionService.Encrypt(note.Value.ToString(), roomPassword);
                    note.EncryptedBalance = encryptionDecryptionService.Encrypt(note.Balance.ToString(), roomPassword);
                }
                return notesToEdit;
            }

            var currencyGroups = noteRepository.GetQuery(note => note.RoomId == updatedNote.RoomId).GroupBy(note => note.Currency);

            var oldCurrencyGroup = currencyGroups.FirstOrDefault(group => group.Key == oldNote.Currency).OrderBy(x => x.DateOfCreation).ToList();
            var newCurrencyGroup = currencyGroups.FirstOrDefault(group => group.Key == updatedNote.Currency).OrderBy(x => x.DateOfCreation).ToList();

            var oldNoteDateOfCreation = oldNote.DateOfCreation;
            oldCurrencyGroup.Remove(oldNote);

            //Пересчитать баланс старых записей

            updatedNote.DateOfCreation = oldNoteDateOfCreation;

            newCurrencyGroup.Add(updatedNote);

            //Пересчитать баланс новых записей

            return oldCurrencyGroup.Concat(newCurrencyGroup);
            //var orderedNotes = noteRepository.GetQuery(note => note.RoomId == updatedNote.RoomId).GroupBy(note => note.Currency)
            //                             .OrderBy(x => x.DateOfCreation).ToList();
            //var noteIndex = orderedNotes.FindIndex(note => note.Id == updatedNote.Id);
            //if (noteIndex < 0)
            //{
            //    return new List<Note>();
            //}
            //var notesToEdit = orderedNotes.Skip(noteIndex - 1).ToList();

            //foreach (var note in notesToEdit)
            //{
            //    note.Value = decimal.Parse(encryptionDecryptionService.Decrypt(note.EncryptedValue, roomPassword));
            //    note.Balance = decimal.Parse(encryptionDecryptionService.Decrypt(note.EncryptedBalance, roomPassword));
            //}

            //if (notesToEdit.Count == 0)
            //{
            //    return notesToEdit;
            //}

            //var index = 0;

            //if (updatedNote.IsDeleted)
            //{
            //    index = noteIndex;
            //    notesToEdit.RemoveAt(noteIndex);
            //}
            //else
            //{
            //    index = notesToEdit.FindIndex(note => note.Id == updatedNote.Id);

            //    notesToEdit[index].Value = updatedNote.Value;
            //    notesToEdit[index].Balance = notesToEdit[0].Balance + updatedNote.Value;
            //    notesToEdit[index].DateOfModification = DateTime.UtcNow;
            //}

            //for (int i = index; i < notesToEdit.Count; i++)
            //{
            //    if (i == 0)
            //    {
            //        notesToEdit[i].Balance = notesToEdit[i].Value;
            //        continue;
            //    }
            //    notesToEdit[i].Balance = notesToEdit[i - 1].Balance + notesToEdit[i].Value;
            //}

            //foreach (var note in notesToEdit)
            //{
            //    note.EncryptedValue = encryptionDecryptionService.Encrypt(note.Value.ToString(), roomPassword);
            //    note.EncryptedBalance = encryptionDecryptionService.Encrypt(note.Balance.ToString(), roomPassword);
            //}
            //return notesToEdit;
        }
    }
}