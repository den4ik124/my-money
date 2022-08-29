using AutoMapper;
using MediatR;
using Notebook.Application.Core;
using Notebook.Application.DTOs;
using Notebook.Core;
using Notebook.Core.Interfaces.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Notebook.Application.Notes.Queries
{
    public class GetNoteByIdQueryHandler : IRequestHandler<GetNoteByIdQuery, Result<NoteDto>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public GetNoteByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<Result<NoteDto>> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
        {
            var noteFromDb = this.unitOfWork.GetGenericRepository<Note>()
                                            .GetQuery(note => note.Id == request.NoteId)
                                            .FirstOrDefault();
            if (noteFromDb == null)
            {
                return Result<NoteDto>.Failure($"Such note (ID : {request.NoteId}) does not exist");
            }
            var response = this.mapper.Map<NoteDto>(noteFromDb);

            return Result<NoteDto>.Success(response);
        }
    }
}