using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Application.Notes.Commands;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Extensions;
using BudgetHistory.Core.Models;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BudgetHistory.Tests.HandlersTests.Notes
{
    public class EditNoteCommandHandlerTest : NotesBaseTest
    {
        [Fact]
        public async Task EditNoteCommandHandler_ShouldUpdate_NoteData()
        {
            var room = await RoomRepoMock.Object.GetQuery().FirstOrDefault().DecryptValues(EncryptionService, Configuration.GetSection(AppSettings.SecretKey).Value);

            var items = new List<Note>()
            {
                new Note()
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.Id,
                },
                new Note()
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.Id,
                },
                new Note()
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.Id,
                },
            };
            foreach (var item in items)
            {
                await item.EncryptValues(EncryptionService, room.Password);
            }

            NoteRepoMock.Setup(rep => rep.Update(It.IsAny<Note>())).Returns((Note updatedItem) =>
            {
                var item = items.FirstOrDefault();
                Mapper.Map(updatedItem, item);
                return true;
            });
            NoteRepoMock.Setup(rep => rep.GetById(It.IsAny<Guid>())).ReturnsAsync(items.FirstOrDefault());

            RoomRepoMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync((Guid id) =>
            {
                return RoomRepoMock.Object.GetAll().GetAwaiter().GetResult().FirstOrDefault(item => item.Id == id);
            });

            var editedNote = NoteRepoMock.Object.GetQuery().FirstOrDefault();
            var checkString = room.Id;

            var editedNoteDto = new NoteDto();
            Mapper.Map(editedNote, editedNoteDto);
            editedNoteDto.UserId = checkString;
            editedNoteDto.RoomId = checkString;

            //Arrange
            var handler = new EditNoteCommandHandler(NoteService, Mapper);

            var request = new EditNoteCommand()
            {
                EditedNote = editedNoteDto,
            };

            //Act
            var result = await handler.Handle(request, CancellationToken.None);

            //Assert
            var firstItem = items.FirstOrDefault();
            result.IsSuccess.ShouldBeTrue();
            firstItem.UserId.ShouldBe(checkString);
            firstItem.RoomId.ShouldBe(checkString);
        }
    }
}