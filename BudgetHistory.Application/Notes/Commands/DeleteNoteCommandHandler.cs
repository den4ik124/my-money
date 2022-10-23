using BudgetHistory.Application.Core;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Services.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Notes.Commands
{
    public class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand, Result<string>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly INoteService noteService;

        public DeleteNoteCommandHandler(IUnitOfWork unitOfWork, INoteService noteService)
        {
            this.unitOfWork = unitOfWork;
            this.noteService = noteService;
        }

        public async Task<Result<string>> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
        {
            await this.unitOfWork.BeginTransactionAsync();

            var result = await noteService.DeleteNote(request.NoteId);

            return result.IsSuccess ? Result<string>.Success(result.Message) : Result<string>.Failure(result.Message);
        }
    }
}