using AutoMapper;
using BudgetHistory.Application.Core;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Notes.Commands
{
    public class EditNoteCommandHandler : IRequestHandler<EditNoteCommand, Result<string>>
    {
        private readonly INoteService noteService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public EditNoteCommandHandler(INoteService noteService, IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.noteService = noteService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<Result<string>> Handle(EditNoteCommand request, CancellationToken cancellationToken)
        {
            var note = this.mapper.Map<Note>(request.EditedNote);

            if (await noteService.UpdateNote(note))
            {
                return Result<string>.Success("Success");
            }
            return Result<string>.Failure($"Note (Id : {request.EditedNote.Id}) edit has been canceled.");
        }
    }
}