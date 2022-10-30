using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Application.Notes.Commands;
using BudgetHistory.Application.Notes.Queries;
using BudgetHistory.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BudgetHistory.API.Controllers
{
    [Authorize(Policy = nameof(Policies.RoomLoggedIn))]
    public class NotesController : BaseApiController
    {
        [HttpPost("notes")]
        public async Task<IActionResult> GetNotes([FromBody] GetNotesRequestDto request /*PagingFilteringDto filteringData*/)
        {
            return HandleResult(await Mediator.Send(new GetNotesQuery() { RoomId = new Guid(request.RoomId), PageParameters = request.PageInfo }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoteById(Guid id)
        {
            return HandleResult(await Mediator.Send(new GetNoteByIdQuery()
            {
                NoteId = id
            }));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNewNote(NoteCreationDto newNoteData)
        {
            return HandleResult(await Mediator.Send(new CreateNoteCommand() { NoteDto = newNoteData }));
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditNote(NoteDto editedNote)
        {
            return HandleResult(await Mediator.Send(new EditNoteCommand()
            {
                EditedNote = editedNote
            }));
        }

        [Authorize(Policy = nameof(Policies.ManagerAccess))]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteNote(Guid id)
        {
            return HandleResult(await Mediator.Send(new DeleteNoteCommand()
            {
                NoteId = id
            }));
        }
    }
}