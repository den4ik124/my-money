using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Application.Notes.Commands;
using BudgetHistory.Core.Models;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace BudgetHistory.Tests.HandlersTests.Notes
{
    public class EditNoteCommandHandlerTest : NotesBaseTest
    {
        [Fact]
        public async void EditNoteCommandHandler_ShouldUpdate_NoteData()
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
                var item = items.FirstOrDefault();
                item = updatedItem;
                return true;
            });

            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Note>()).Returns(genRepoMock.Object);
            var editedNote = genRepoMock.Object.GetQuery().FirstOrDefault();
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
            var changedNote = genRepoMock.Object.GetQuery().FirstOrDefault();

            //Assert
            result.IsSuccess.ShouldBeTrue();
            changedNote.UserId.ShouldBe(checkString);
            changedNote.RoomId.ShouldBe(checkString);
        }
    }
}