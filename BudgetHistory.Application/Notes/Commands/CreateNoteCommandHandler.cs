using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Notes.Commands
{
    public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, Result<string>>
    {
        private readonly IMapper mapper;
        private readonly INoteService noteService;
        private readonly IEncryptionDecryption encryptionDecryptionService;
        private readonly IConfiguration config;
        private readonly IGenericRepository<Room> roomRepository;

        public CreateNoteCommandHandler(IMapper mapper, INoteService noteService, IEncryptionDecryption encryptionDecryptionService, IConfiguration config, IUnitOfWork unitOfWork)
        {
            this.mapper = mapper;
            this.noteService = noteService;
            this.encryptionDecryptionService = encryptionDecryptionService;
            this.config = config;
            this.roomRepository = unitOfWork.GetGenericRepository<Room>();
        }

        public async Task<Result<string>> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
        {
            var room = await roomRepository.GetById(request.NoteDto.RoomId);
            var decryptedPassword = encryptionDecryptionService.Decrypt(room.Password, config.GetSection(AppSettings.SecretKey).Value);

            var noteModel = this.mapper.Map<Note>(request.NoteDto);

            var currencyEnum = Enum.Parse<Currency>(request.NoteDto.Currency);
            var result = await noteService.CreateNewNote(noteModel, currencyEnum, request.NoteDto.Value, request.NoteDto.RoomId, decryptedPassword);

            return result.IsSuccess ? Result<string>.Success(result.Message) : Result<string>.Failure(result.Message);
        }
    }
}