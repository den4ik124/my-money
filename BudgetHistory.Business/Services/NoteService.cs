using AutoMapper;
using BudgetHistory.Abstractions.Interfaces;
using BudgetHistory.Abstractions.Interfaces.Data;
using BudgetHistory.Abstractions.Services;
using BudgetHistory.Core;
using BudgetHistory.Core.Extensions;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Resources;
using BudgetHistory.Core.Services.Responses;
using BudgetHistory.Logging;
using BudgetHistory.Logging.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BudgetHistory.Business.Services
{
    public class NoteService : BaseService, INoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRoomService _roomService;
        private readonly IEncryptionDecryption _encryptionDecryptionService;
        private readonly CustomLogger _logger;
        private readonly IGenericRepository<Note> _noteRepository;

        public NoteService(IUnitOfWork unitOfWork, IMapper mapper, IRoomService roomService, IEncryptionDecryption encryptionDecryption, ICustomLoggerFactory loggerFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _roomService = roomService;
            _encryptionDecryptionService = encryptionDecryption;
            _logger = loggerFactory.CreateLogger<NoteService>();
            _noteRepository = unitOfWork.GetGenericRepository<Note>();
        }

        public async Task<ServiceResponse<IEnumerable<Note>>> GetAllNotes(Guid roomId, int pageNumber, int pageSize, Expression<Func<Note, bool>> predicate = null, Func<IQueryable<Note>, IOrderedQueryable<Note>> orderBy = null)
        {
            var notes = GetItemsFromQuery(pageNumber, pageSize, predicate, orderBy);
            var room = (await _roomService.GetRoomById(roomId))?.Value;
            if (room is null)
            {
                return await base.Failed<IEnumerable<Note>>(_logger, string.Format(ResponseMessages.RoomDoesNotExist, roomId));
            }

            foreach (var note in notes)
            {
                await note.DecryptValues(_encryptionDecryptionService, room.Password);
            }

            return ServiceResponse<IEnumerable<Note>>.Success(notes.OrderBy(note => note.DateOfCreation));
        }

        public async Task<ServiceResponse<Note>> GetNoteById(Guid noteId)
        {
            var note = _noteRepository.GetQuery(note => note.Id == noteId).FirstOrDefault();
            if (note is null)
            {
                return await Failed<Note>(_logger, string.Format(ResponseMessages.NoteDoesNotExist, noteId));
            }

            var room = (await _roomService.GetRoomById(note.RoomId)).Value;

            return ServiceResponse<Note>.Success(await note.DecryptValues(_encryptionDecryptionService, room.Password));
        }

        public async Task<ServiceResponse> CreateNewNote(Note newNote, Currency currency, decimal value)
        {
            var errorMessage = string.Empty;
            var room = (await _roomService.GetRoomById(newNote.RoomId)).Value;

            var lastNote = _noteRepository.GetQuery(note => note.RoomId == newNote.RoomId
                                     && note.Currency == currency,
                                     order => order.OrderBy(note => note.DateOfCreation))?.LastOrDefault();
            if (lastNote is not null)
            {
                await lastNote.DecryptValues(_encryptionDecryptionService, room.Password);
                newNote.Balance = lastNote.Balance + value;
            }
            else
            {
                if (value < 0)
                {
                    return await Failed(_logger, ResponseMessages.NegativeBalanceError);
                }
                newNote.Balance = value;
            }

            newNote.Id = Guid.NewGuid();
            await newNote.EncryptValues(_encryptionDecryptionService, room.Password);

            if (await _noteRepository.Add(newNote))
            {
                await _unitOfWork.CompleteAsync();
                return ServiceResponse.Success(string.Format(ResponseMessages.NoteSuccessfullyCreated, newNote.Id));
            }
            return await Failed(_logger, ResponseMessages.NoteCreationError);
        }

        public async Task<ServiceResponse> DeleteNote(Guid noteId)
        {
            return ServiceResponse.Failure("Method not implemented properly.");
            //return await base.Failed(_logger, "Method not implemented properly.");
        }

        public async Task<ServiceResponse> UpdateNote(Note updatedNote)
        {
            await _unitOfWork.BeginTransactionAsync();

            var oldNote = await _noteRepository.GetById(updatedNote.Id);
            if (oldNote is null)
            {
                return await Failed(_logger, string.Format(ResponseMessages.NoteDoesNotExist, updatedNote.Id));
            }

            var room = (await _roomService.GetRoomById(updatedNote.RoomId)).Value;

            await oldNote.DecryptValues(_encryptionDecryptionService, room.Password);
            await updatedNote.EncryptValues(_encryptionDecryptionService, room.Password);

            updatedNote.DateOfModification = DateTime.UtcNow;
            updatedNote.Balance = oldNote.Balance;

            if (oldNote.Value == updatedNote.Value
                && oldNote.IsDeleted == updatedNote.IsDeleted
                && oldNote.Currency == updatedNote.Currency)
            {
                _mapper.Map(updatedNote, oldNote);
                _noteRepository.Update(oldNote);
                _unitOfWork.TransactionCommit();
                await _unitOfWork.CompleteAsync();
                return ServiceResponse.Success(string.Format(ResponseMessages.NoteSuccessfullyUpdated, updatedNote.Id));
            }

            try
            {
                var notesToEdit = await GetNotesWithUpdatedBalances(oldNote, updatedNote, room.Password);

                if (!notesToEdit.Any())
                {
                    _unitOfWork.RollbackTransaction();
                    return await Failed(_logger, ResponseMessages.NotesToUpdateListIsEmpty);
                }

                foreach (var note in notesToEdit)
                {
                    _noteRepository.Update(note);
                }
            }
            catch (NoteNegativeBalanceException ex)
            {
                return ServiceResponse.Failure(ex.Message);
            }

            _unitOfWork.TransactionCommit();
            await _unitOfWork.CompleteAsync();
            return ServiceResponse.Success(string.Format(ResponseMessages.NoteSuccessfullyUpdated, updatedNote.Id));
        }

        private async Task<IEnumerable<Note>> GetNotesWithUpdatedBalances(Note oldNote, Note updatedNote, string roomPassword)
        {
            if (oldNote.Currency == updatedNote.Currency)
            {
                return await RecalculateNotesWithSameCurrency(oldNote, updatedNote, roomPassword);
            }

            return await RecalculateNotesWithDifferentCurrencies(oldNote, updatedNote, roomPassword); ;
        }

        private async Task<IEnumerable<Note>> RecalculateNotesWithDifferentCurrencies(Note oldNote, Note updatedNote, string roomPassword)
        {
            var currencyGroups = _noteRepository.GetQuery(note => note.RoomId == updatedNote.RoomId
                                                                          && note.DateOfCreation >= oldNote.DateOfCreation
                                                                          && !note.IsDeleted,
                                                         note => note.OrderBy(item => item.DateOfCreation))
                                                           .AsEnumerable()
                                                           .GroupBy(note => note.Currency);
            var notes = currencyGroups.SelectMany(item => item);
            foreach (var item in notes)
            {
                await item.DecryptValues(_encryptionDecryptionService, roomPassword);
            }
            //TODO: проверить ошибку тут, если записи будут браться некорректно из групп

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
                var lastNoteFromNewCurrencyGroup = _noteRepository.GetQuery(note => note.RoomId == updatedNote.RoomId
                                                              && note.Currency == updatedNote.Currency
                                                              && !note.IsDeleted,
                                             note => note.OrderBy(item => item.DateOfCreation)).LastOrDefault();
                if (lastNoteFromNewCurrencyGroup is not null)
                {
                    await lastNoteFromNewCurrencyGroup.DecryptValues(_encryptionDecryptionService, roomPassword);
                    newGroupInitialBalance = lastNoteFromNewCurrencyGroup.Balance;
                }
            }

            var oldGroupFirstElement = oldCurrencyGroup.FirstOrDefault();
            var oldGroupInitialBalance = oldGroupFirstElement is null ? 0 : oldGroupFirstElement.Balance - oldGroupFirstElement.Value;

            oldCurrencyGroup.Remove(oldNote);
            if (oldCurrencyGroup.Count > 0)
            {
                oldCurrencyGroup = await RecalculateBalances(oldCurrencyGroup, oldGroupInitialBalance, roomPassword);
            }

            _mapper.Map(updatedNote, oldNote);

            newCurrencyGroup.Insert(0, oldNote);

            newCurrencyGroup = await RecalculateBalances(newCurrencyGroup, newGroupInitialBalance, roomPassword);

            return oldCurrencyGroup.Concat(newCurrencyGroup); ;
        }

        private async Task<IEnumerable<Note>> RecalculateNotesWithSameCurrency(Note oldNote, Note updatedNote, string roomPassword)
        {
            var notesToUpdate = _noteRepository.GetQuery(note => note.RoomId == updatedNote.RoomId
                                                     && note.DateOfCreation >= oldNote.DateOfCreation
                                                     && note.Currency == oldNote.Currency,
                                                     _ => _.OrderBy(note => note.DateOfCreation));

            foreach (var item in notesToUpdate)
            {
                await item.DecryptValues(_encryptionDecryptionService, roomPassword);
            }

            var firstNote = (await notesToUpdate.FirstOrDefault()?.DecryptValues(_encryptionDecryptionService, roomPassword))
                ?? throw new Exception("В списке записей - нет записей! Проверь запрос и всё ли в порядке с записями в БД.");

            var groupInitialBalance = firstNote.Balance - firstNote.Value;

            updatedNote.DateOfCreation = oldNote.DateOfCreation;
            _mapper.Map(updatedNote, oldNote);

            return await RecalculateBalances(notesToUpdate, groupInitialBalance, roomPassword);
        }

        private async Task<List<Note>> RecalculateBalances(IEnumerable<Note> notes, decimal initialBalance, string roomPassword)
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
                    throw new NoteNegativeBalanceException($"Изменение в записи (id:{note.Id})\nведет к отрицательному балансу в следующих записях. Проверьте валидность указанного значения."); //TODO поменять на ресурс
                }

                await note.EncryptValues(_encryptionDecryptionService, roomPassword);
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
                return _noteRepository.GetQuery(predicate, orderBy)
                           .Skip(offset)
                           .Take(pageSize);
            }
            return _noteRepository.GetQuery(null, orderBy)
                       .Skip(offset)
                       .Take(pageSize);
        }
    }
}