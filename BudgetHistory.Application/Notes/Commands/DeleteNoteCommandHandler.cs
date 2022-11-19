using BudgetHistory.Abstractions.Interfaces.Data;
using BudgetHistory.Abstractions.Services;
using BudgetHistory.Application.Core;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Notes.Commands
{
    public class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INoteService _noteService;

        public DeleteNoteCommandHandler(IUnitOfWork unitOfWork, INoteService noteService)
        {
            _unitOfWork = unitOfWork;
            _noteService = noteService;
        }

        public async Task<Result<string>> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();

            var result = await _noteService.DeleteNote(request.NoteId);

            return result.IsSuccess ? Result<string>.Success(result.Message) : Result<string>.Failure(result.Message);
        }
    }
}