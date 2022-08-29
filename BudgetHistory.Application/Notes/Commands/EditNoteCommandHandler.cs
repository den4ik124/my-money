using AutoMapper;
using MediatR;
using Notebook.Application.Core;
using Notebook.Core;
using Notebook.Core.Interfaces.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Notebook.Application.Notes.Commands
{
    public class EditNoteCommandHandler : IRequestHandler<EditNoteCommand, Result<string>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public EditNoteCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<Result<string>> Handle(EditNoteCommand request, CancellationToken cancellationToken)
        {
            await this.unitOfWork.BeginTransactionAsync();
            var repository = this.unitOfWork.GetGenericRepository<Note>();
            var noteFromDb = repository.GetQuery(note => note.Id == request.EditedNote.Id).FirstOrDefault();
            if (noteFromDb == null)
            {
                return Result<string>.Failure($"Such note does not exists \n(Id = {request.EditedNote.Id}).");
            }
            this.mapper.Map(request.EditedNote, noteFromDb);
            if (!repository.Update(noteFromDb))
            {
                return Result<string>.Failure($"Note (Id : {request.EditedNote.Id}) edit has been canceled.");
            }

            if (await this.unitOfWork.CompleteAsync())
            {
                this.unitOfWork.TransactionCommit();
                return Result<string>.Success("Success");
            }

            return Result<string>.Failure($"Note (Id : {request.EditedNote.Id}) edit has been canceled.");
        }
    }
}