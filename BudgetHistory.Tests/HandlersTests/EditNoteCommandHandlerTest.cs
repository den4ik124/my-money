using Moq;
using Notebook.Application.DTOs;
using Notebook.Application.Notes.Commands;
using Notebook.Core;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Notebook.Tests.HandlersTests
{
    public class EditNoteCommandHandlerTest : NotesBaseTest
    {
        [Theory]
        [InlineData("")]
        public async void EditNoteCommandHandler_ShouldUpdate_NoteData(Guid noteId)
        {
            var genRepoMock = Mocks.MockRepository.GetMockedNoteRepository();
            var items = new List<Note>()
            {
                new Note()
                {
                    Id = Guid.NewGuid(),
                },
                new Note()
                {
                    Id = Guid.NewGuid(),
                },
                new Note()
                {
                    Id = Guid.NewGuid(),
                },
            };

            genRepoMock.Setup(rep => rep.Update(It.IsAny<Note>())).Returns((Note updatedItem) =>
            {
                var item = items.FirstOrDefault(i => i.Id == noteId);
                item = updatedItem;
                return true;
            });
            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Note>()).Returns(genRepoMock.Object);
            var editedNote = (await genRepoMock.Object.GetWhere(x => x.Id == noteId)).FirstOrDefault();
            var checkString = Guid.NewGuid();

            var editedNoteDto = new NoteDto();
            Mapper.Map(editedNote, editedNoteDto);

            editedNoteDto.UserId = checkString;
            editedNoteDto.RoomId = checkString;

            //Arrange
            var handler = new EditNoteCommandHandler(UnitOfWorkMock.Object, Mapper);

            var request = new EditNoteCommand()
            {
                EditedNote = editedNoteDto,
            };

            //Act
            var result = await handler.Handle(request, CancellationToken.None);
            var changedNote = (await genRepoMock.Object.GetWhere(x => x.Id == noteId)).FirstOrDefault();

            //Assert
            result.IsSuccess.ShouldBeTrue();
            changedNote.UserId.ShouldBe(checkString);
            changedNote.RoomId.ShouldBe(checkString);
        }
    }
}