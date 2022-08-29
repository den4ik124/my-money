using AutoMapper;
using MediatR;
using Notebook.Application.Core;
using Notebook.Core;
using Notebook.Core.Interfaces.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Notebook.Application.Notes.Commands
{
    public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, Result<string>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public CreateNoteCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<Result<string>> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
        {
            var repo = this.unitOfWork.GetGenericRepository<Note>();
            request.NoteDto.Id = Guid.NewGuid();
            var currentBalance = repo.GetQuery().OrderBy(x => x.DateOfCreation).Last().Balance;
            request.NoteDto.Balance = currentBalance + request.NoteDto.Value;

            var noteModel = this.mapper.Map<Note>(request.NoteDto);
            if (await repo.Add(noteModel))
            {
                await this.unitOfWork.CompleteAsync();
                return Result<string>.Success("Creation succeeded.");
            }
            return Result<string>.Failure("Creation failed.");
        }
    }
}