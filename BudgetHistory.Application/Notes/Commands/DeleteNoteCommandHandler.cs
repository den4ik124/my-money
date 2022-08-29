using MediatR;
using Notebook.Application.Core;
using Notebook.Core;
using Notebook.Core.Interfaces.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Notebook.Application.Notes.Commands
{
    public class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand, Result<string>>
    {
        private readonly IUnitOfWork unitOfWork;

        public DeleteNoteCommandHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
        {
            await this.unitOfWork.BeginTransactionAsync();
            var repository = this.unitOfWork.GetGenericRepository<Note>();
            var noteFromDb = repository.GetQuery(n => n.Id == request.NoteId).FirstOrDefault();
            if (noteFromDb == null)
            {
                return Result<string>.Failure($"Such note does not exists \n(Id = {request.NoteId}).");
            }

            if (repository.Delete(noteFromDb))
            {
                this.unitOfWork.TransactionCommit();
                await this.unitOfWork.CompleteAsync();
                return Result<string>.Success($"Note (Id : {request.NoteId}) has been removed.");
            }
            this.unitOfWork.RollbackTransaction();
            return Result<string>.Failure($"Note (Id : {request.NoteId}) removing has failed.");
        }
    }
}