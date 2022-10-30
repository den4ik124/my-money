using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Notes.Commands
{
    public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, Result<string>>
    {
        private readonly IMapper _mapper;
        private readonly INoteService _noteService;

        public CreateNoteCommandHandler(IMapper mapper, INoteService noteService)
        {
            _mapper = mapper;
            _noteService = noteService;
        }

        public async Task<Result<string>> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
        {
            var noteModel = _mapper.Map<Note>(request.NoteDto);

            var currencyEnum = Enum.Parse<Currency>(request.NoteDto.Currency);
            var result = await _noteService.CreateNewNote(noteModel, currencyEnum, request.NoteDto.Value);

            return result.IsSuccess ? Result<string>.Success(result.Message) : Result<string>.Failure(result.Message);
        }
    }
}