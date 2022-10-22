using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Extensions;
using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Notes.Queries
{
    public class GetNotesQueryHandler : IRequestHandler<GetNotesQuery, Result<PagedList<NoteDto>>>
    {
        private readonly IMapper mapper;
        private readonly IEncryptionDecryption encryptionDecryptionService;
        private readonly IConfiguration config;
        private readonly IGenericRepository<Note> noteRepository;
        private readonly IGenericRepository<Room> roomRepository;

        public GetNotesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IEncryptionDecryption encryptionDecryptionService, IConfiguration config)
        {
            this.mapper = mapper;
            this.encryptionDecryptionService = encryptionDecryptionService;
            this.config = config;
            this.noteRepository = unitOfWork.GetGenericRepository<Note>();
            this.roomRepository = unitOfWork.GetGenericRepository<Room>();
        }

        public async Task<Result<PagedList<NoteDto>>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
        {
            var notes = GetItemsFromQuery(request);

            var room = await roomRepository.GetById(request.RoomId);
            var decryptedPassword = encryptionDecryptionService.Decrypt(room.Password, config.GetSection(AppSettings.SecretKey).Value);

            foreach (var note in notes)
            {
                note.DecryptValues(encryptionDecryptionService, decryptedPassword);

                //note.Value = encryptionDecryptionService.DecryptToDecimal(note.EncryptedValue, decryptedPassword);
                //note.Balance = encryptionDecryptionService.DecryptToDecimal(note.EncryptedBalance, decryptedPassword);
            }

            var response = this.mapper.Map<IEnumerable<NoteDto>>(notes);

            request.PageParameters.Items = await this.noteRepository.GetItemsCount();
            return Result<PagedList<NoteDto>>.Success(new PagedList<NoteDto>(response, request.PageParameters));
        }

        private IEnumerable<Note> GetItemsFromQuery(GetNotesQuery request,
                                                    Expression<Func<Note, bool>> predicate = null,
                                                    Func<IQueryable<Note>, IOrderedQueryable<Note>> orderBy = null)
        {
            var offset = (request.PageParameters.Page - 1) * request.PageParameters.Size;
            if (predicate != null)
            {
                return this.noteRepository.GetQuery(predicate, orderBy)
                           .Skip(offset)
                           .Take(request.PageParameters.Size);
            }
            return this.noteRepository.GetQuery(null, orderBy)
                       .Skip(offset)
                       .Take(request.PageParameters.Size);
        }
    }
}