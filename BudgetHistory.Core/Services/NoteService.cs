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

        public async Task<IEnumerable<Note>> GetAllNotes(Guid roomId, int pageNumber, int pageSize, Expression<Func<Note, bool>> predicate = null, Func<IQueryable<Note>, IOrderedQueryable<Note>> orderBy = null)
        {
            var notes = GetItemsFromQuery(pageNumber, pageSize, predicate, orderBy);

            var room = await roomRepository.GetById(roomId);
            var decryptedPassword = encryptionDecryptionService.Decrypt(room.Password, configuration.GetSection(Constants.AppSettings.SecretKey).Value);

            foreach (var note in notes)
            {
                note.DecryptValues(encryptionDecryptionService, decryptedPassword);
            }

            return notes.OrderBy(note => note.DateOfCreation);
        }

        public async Task<NoteServiceResponse> CreateNewNote(Note newNote, Currency currency, decimal value, Guid roomId, string roomPassword)
        {
            var lastNote = noteRepository.GetQuery(note => note.RoomId == roomId
                                     && note.Currency == currency,
                                     order => order.OrderBy(note => note.DateOfCreation))?.LastOrDefault();
            if (lastNote is not null)
            {
                lastNote.DecryptValues(encryptionDecryptionService, roomPassword);
                newNote.Balance = lastNote.Balance + value;
            }
            else
            {
                if (value < 0)
                {
                    return new NoteServiceResponse() { IsSuccess = false, Message = "Баланс не может иметь отрицательное значение!" };
                }
                newNote.Balance = value;
            }

            newNote.Id = Guid.NewGuid();
            newNote.EncryptValues(encryptionDecryptionService, roomPassword);

            if (await noteRepository.Add(newNote))
            {
                await this.unitOfWork.CompleteAsync();
                return new NoteServiceResponse() { IsSuccess = true, Message = $"Note (id : {newNote.Id})\nhas been created successfully!" }; ;
            }
            return new NoteServiceResponse() { IsSuccess = false, Message = "Note was not created." }; ;
        }

        public async Task<NoteServiceResponse> DeleteNote(Guid noteId)
        {
            return new NoteServiceResponse() { IsSuccess = false, Message = "Method not implemented properly." };
        }

        public async Task<NoteServiceResponse> UpdateNote(Note updatedNote)
        {
            await this.unitOfWork.BeginTransactionAsync();

            var oldNote = await noteRepository.GetById(updatedNote.Id);
            if (oldNote is null)
            {
                return new NoteServiceResponse() { IsSuccess = false, Message = $"Note (id : {updatedNote.Id}\nwas not found." };
            }

            var room = await roomRepository.GetById(updatedNote.RoomId);
            var decryptedPassword = encryptionDecryptionService.Decrypt(room.Password, configuration.GetSection(Constants.AppSettings.SecretKey).Value);

            oldNote.DecryptValues(encryptionDecryptionService, decryptedPassword);
            updatedNote.EncryptValues(encryptionDecryptionService, decryptedPassword);

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
                return new NoteServiceResponse() { IsSuccess = true, Message = $"Note (id : {updatedNote.Id}\nhas been updated successfully." };
            }

            try
            {
                var notesToEdit = GetNotesWithUpdatedBalances(oldNote, updatedNote, decryptedPassword);

                if (!notesToEdit.Any())
                {
                    this.unitOfWork.RollbackTransaction();
                    return new NoteServiceResponse() { IsSuccess = false, Message = $"There are no notes to be updated." };
                }

                foreach (var note in notesToEdit)
                {
                    noteRepository.Update(note);
                }
            }
            catch (NoteNegativeBalanceException ex)
            {
                return new NoteServiceResponse() { IsSuccess = false, Message = ex.Message };
            }

            this.unitOfWork.TransactionCommit();
            await this.unitOfWork.CompleteAsync();
            return new NoteServiceResponse() { IsSuccess = true, Message = $"Note (id : {updatedNote.Id}\nhas been updated successfully." };
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
                var newGroupFirstElement = newCurrencyGroup.FirstOrDefault(); //получили первый элемент новой коллекции
                newGroupInitialBalance = newGroupFirstElement.Balance - newGroupFirstElement.Value; //запомнили -1 баланс
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

            var oldGroupFirstElement = oldCurrencyGroup.FirstOrDefault(); //получили первый элемент новой коллекции
            var oldGroupInitialBalance = oldGroupFirstElement is null ? 0 : oldGroupFirstElement.Balance - oldGroupFirstElement.Value; //запомнили -1 баланс

            oldCurrencyGroup.Remove(oldNote);

            //Пересчитать баланс старых записей
            oldCurrencyGroup = RecalculateBalances(oldCurrencyGroup.ToList(), oldGroupInitialBalance, roomPassword);

            this.mapper.Map(updatedNote, oldNote);

            newCurrencyGroup.Insert(0, oldNote);

            //Пересчитать баланс новых записей
            newCurrencyGroup = RecalculateBalances(newCurrencyGroup, newGroupInitialBalance, roomPassword);

            var concatedList = oldCurrencyGroup.Concat(newCurrencyGroup);
            return concatedList;
        }

        private IEnumerable<Note> RecalculateNotesWithSameCurrency(Note oldNote, Note updatedNote, string roomPassword)
        {
            var notesToUpdate = noteRepository.GetQuery(note => note.RoomId == updatedNote.RoomId
                                                     && note.DateOfCreation >= oldNote.DateOfCreation
                                                     && note.Currency == oldNote.Currency);

            foreach (var item in notesToUpdate)
            {
                item.DecryptValues(encryptionDecryptionService, roomPassword);
            }

            var firstNote = notesToUpdate.FirstOrDefault()?.DecryptValues(encryptionDecryptionService, roomPassword)
                ?? throw new Exception("В списке записей - нет записей! Проверь запрос и всё ли в порядке с записями в БД."); ;

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