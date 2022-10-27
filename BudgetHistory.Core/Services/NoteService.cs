using AutoMapper;
using BudgetHistory.Core.Extensions;
using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using BudgetHistory.Core.Services.Responses;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BudgetHistory.Core.Services
{
    public class NoteService : INoteService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IRoomService roomService;
        private readonly IEncryptionDecryption encryptionDecryptionService;
        private readonly IGenericRepository<Note> noteRepository;

        public NoteService(IUnitOfWork unitOfWork, IMapper mapper, IRoomService roomService, IEncryptionDecryption encryptionDecryption)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.roomService = roomService;
            this.encryptionDecryptionService = encryptionDecryption;
            this.noteRepository = unitOfWork.GetGenericRepository<Note>();
        }

        public async Task<IEnumerable<Note>> GetAllNotes(Guid roomId, int pageNumber, int pageSize, Expression<Func<Note, bool>> predicate = null, Func<IQueryable<Note>, IOrderedQueryable<Note>> orderBy = null)
        {
            var notes = GetItemsFromQuery(pageNumber, pageSize, predicate, orderBy);
            var room = (await roomService.GetRoomById(roomId)).Value;

            foreach (var note in notes)
            {
                note.DecryptValues(encryptionDecryptionService, room.Password);
            }

            return notes.OrderBy(note => note.DateOfCreation);
        }

        public async Task<ServiceResponse<Note>> GetNoteById(Guid noteId)
        {
            var note = noteRepository.GetQuery(note => note.Id == noteId).FirstOrDefault()
                ?? throw new ArgumentNullException($"Note (ID : {noteId}) does not exist");

            var room = (await roomService.GetRoomById(note.RoomId)).Value;

            return new ServiceResponse<Note>() { IsSuccess = true, Value = note.DecryptValues(encryptionDecryptionService, room.Password) };
        }

        public async Task<ServiceResponse> CreateNewNote(Note newNote, Currency currency, decimal value)
        {
            var room = (await roomService.GetRoomById(newNote.RoomId)).Value;

            var lastNote = noteRepository.GetQuery(note => note.RoomId == newNote.RoomId
                                     && note.Currency == currency,
                                     order => order.OrderBy(note => note.DateOfCreation))?.LastOrDefault();
            if (lastNote is not null)
            {
                lastNote.DecryptValues(encryptionDecryptionService, room.Password);
                newNote.Balance = lastNote.Balance + value;
            }
            else
            {
                if (value < 0)
                {
                    return new ServiceResponse() { IsSuccess = false, Message = "Баланс не может иметь отрицательное значение!" };
                }
                newNote.Balance = value;
            }

            newNote.Id = Guid.NewGuid();
            newNote.EncryptValues(encryptionDecryptionService, room.Password);

            if (await noteRepository.Add(newNote))
            {
                await this.unitOfWork.CompleteAsync();
                return new ServiceResponse() { IsSuccess = true, Message = $"Note (id : {newNote.Id})\nhas been created successfully!" }; ;
            }
            return new ServiceResponse() { IsSuccess = false, Message = "Note was not created." }; ;
        }

        public async Task<ServiceResponse> DeleteNote(Guid noteId)
        {
            return new ServiceResponse() { IsSuccess = false, Message = "Method not implemented properly." };
        }

        public async Task<ServiceResponse> UpdateNote(Note updatedNote)
        {
            await this.unitOfWork.BeginTransactionAsync();

            var oldNote = await noteRepository.GetById(updatedNote.Id);
            if (oldNote is null)
            {
                return new ServiceResponse() { IsSuccess = false, Message = $"Note (id : {updatedNote.Id}\nwas not found." };
            }

            var room = (await roomService.GetRoomById(updatedNote.RoomId)).Value;

            oldNote.DecryptValues(encryptionDecryptionService, room.Password);
            updatedNote.EncryptValues(encryptionDecryptionService, room.Password);

            updatedNote.DateOfModification = DateTime.UtcNow;
            updatedNote.Balance = oldNote.Balance;

            if (oldNote.Value == updatedNote.Value
                && oldNote.IsDeleted == updatedNote.IsDeleted
                && oldNote.Currency == updatedNote.Currency)
            {
                this.mapper.Map(updatedNote, oldNote);
                noteRepository.Update(oldNote);
                this.unitOfWork.TransactionCommit();
                await this.unitOfWork.CompleteAsync();
                return new ServiceResponse() { IsSuccess = true, Message = $"Note (id : {updatedNote.Id}\nhas been updated successfully." };
            }

            try
            {
                var notesToEdit = GetNotesWithUpdatedBalances(oldNote, updatedNote, room.Password);

                if (!notesToEdit.Any())
                {
                    this.unitOfWork.RollbackTransaction();
                    return new ServiceResponse() { IsSuccess = false, Message = $"There are no notes to be updated." };
                }

                foreach (var note in notesToEdit)
                {
                    noteRepository.Update(note);
                }
            }
            catch (NoteNegativeBalanceException ex)
            {
                return new ServiceResponse() { IsSuccess = false, Message = ex.Message };
            }

            this.unitOfWork.TransactionCommit();
            await this.unitOfWork.CompleteAsync();
            return new ServiceResponse() { IsSuccess = true, Message = $"Note (id : {updatedNote.Id}\nhas been updated successfully." };
        }

        private IEnumerable<Note> GetNotesWithUpdatedBalances(Note oldNote, Note updatedNote, string roomPassword)
        {
            if (oldNote.Currency == updatedNote.Currency)
            {
                return RecalculateNotesWithSameCurrency(oldNote, updatedNote, roomPassword);
            }

            return RecalculateNotesWithDifferentCurrencies(oldNote, updatedNote, roomPassword); ;
        }

        private IEnumerable<Note> RecalculateNotesWithDifferentCurrencies(Note oldNote, Note updatedNote, string roomPassword)
        {
            var currencyGroups = noteRepository.GetQuery(note => note.RoomId == updatedNote.RoomId
                                                                          && note.DateOfCreation >= oldNote.DateOfCreation
                                                                          && !note.IsDeleted,
                                                         note => note.OrderBy(item => item.DateOfCreation))
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
                                                 .ToList();

            updatedNote.DateOfCreation = oldNote.DateOfCreation;

            var newGroupInitialBalance = 0m;

            var newCurrencyGroup = new List<Note>();

            if (currencyGroups.Any(group => group.Key == updatedNote.Currency))
            {
                newCurrencyGroup = currencyGroups.FirstOrDefault(group => group.Key == updatedNote.Currency)
                                                 .ToList();
                var newGroupFirstElement = newCurrencyGroup.FirstOrDefault();
                newGroupInitialBalance = newGroupFirstElement.Balance - newGroupFirstElement.Value;
            }
            else
            {
                var lastNoteFromNewCurrencyGroup = noteRepository.GetQuery(note => note.RoomId == updatedNote.RoomId
                                                              && note.Currency == updatedNote.Currency
                                                              && !note.IsDeleted,
                                             note => note.OrderBy(item => item.DateOfCreation)).LastOrDefault();
                if (lastNoteFromNewCurrencyGroup is not null)
                {
                    lastNoteFromNewCurrencyGroup.DecryptValues(encryptionDecryptionService, roomPassword);
                    newGroupInitialBalance = lastNoteFromNewCurrencyGroup.Balance;
                }
            }

            var oldGroupFirstElement = oldCurrencyGroup.FirstOrDefault(); 
            var oldGroupInitialBalance = oldGroupFirstElement is null ? 0 : oldGroupFirstElement.Balance - oldGroupFirstElement.Value;

            oldCurrencyGroup.Remove(oldNote);
            if (oldCurrencyGroup.Count > 0)
            {
                oldCurrencyGroup = RecalculateBalances(oldCurrencyGroup, oldGroupInitialBalance, roomPassword);
            }

            this.mapper.Map(updatedNote, oldNote);

            newCurrencyGroup.Insert(0, oldNote);

            newCurrencyGroup = RecalculateBalances(newCurrencyGroup, newGroupInitialBalance, roomPassword);

            return oldCurrencyGroup.Concat(newCurrencyGroup); ;
        }

        private IEnumerable<Note> RecalculateNotesWithSameCurrency(Note oldNote, Note updatedNote, string roomPassword)
        {
            var notesToUpdate = noteRepository.GetQuery(note => note.RoomId == updatedNote.RoomId
                                                     && note.DateOfCreation >= oldNote.DateOfCreation
                                                     && note.Currency == oldNote.Currency,
                                                     _ => _.OrderBy(note => note.DateOfCreation));

            foreach (var item in notesToUpdate)
            {
                item.DecryptValues(encryptionDecryptionService, roomPassword);
            }

            var firstNote = notesToUpdate.FirstOrDefault()?.DecryptValues(encryptionDecryptionService, roomPassword)
                ?? throw new Exception("В списке записей - нет записей! Проверь запрос и всё ли в порядке с записями в БД.");

            var groupInitialBalance = firstNote.Balance - firstNote.Value;

            updatedNote.DateOfCreation = oldNote.DateOfCreation;
            this.mapper.Map(updatedNote, oldNote);

            return RecalculateBalances(notesToUpdate, groupInitialBalance, roomPassword);
        }

        private List<Note> RecalculateBalances(IEnumerable<Note> notes, decimal initialBalance, string roomPassword)
        {
            var currentBalance = initialBalance;
            var firstNoteId = notes.First().Id;
            foreach (var note in notes)
            {
                if (note.Id == firstNoteId)
                {
                    if (note.IsDeleted)
                    {
                        currentBalance = initialBalance;
                    }
                    else
                    {
                        currentBalance = note.Balance = initialBalance + note.Value;
                    }
                }
                else
                {
                    if (!note.IsDeleted)
                    {
                        currentBalance = note.Balance = currentBalance + note.Value;
                    }
                }


            if (note.Balance < 0)
                {
                    throw new NoteNegativeBalanceException($"Изменение в записи (id:{note.Id})\nведет к отрицательному балансу в следующих записях. Проверьте валидность указанного значения.");
                }

                note.EncryptValues(encryptionDecryptionService, roomPassword);
            }
            return notes.ToList();
        }

        private IEnumerable<Note> GetItemsFromQuery(int pageNumber, int pageSize,
                                                   Expression<Func<Note, bool>> predicate = null,
                                                   Func<IQueryable<Note>, IOrderedQueryable<Note>> orderBy = null)
        {
            var offset = (pageNumber - 1) * pageSize;
            if (predicate != null)
            {
                return this.noteRepository.GetQuery(predicate, orderBy)
                           .Skip(offset)
                           .Take(pageSize);
            }
            return this.noteRepository.GetQuery(null, orderBy)
                       .Skip(offset)
                       .Take(pageSize);
        }
    }
}