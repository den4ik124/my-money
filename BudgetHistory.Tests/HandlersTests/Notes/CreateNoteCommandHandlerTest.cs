using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Application.Notes.Commands;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services;
using Shouldly;
using System;
using System.Threading;
using Xunit;

namespace BudgetHistory.Tests.HandlersTests.Notes
{
    public class CreateNoteCommandHandlerTest : NotesBaseTest
    {
        [Fact]
        public async void CreateNoteCommandHandler_ShouldIncrease_NotesCount()
        {
            var genRepoMock = Mocks.MockRepository.GetMockedNoteRepository();
            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Note>()).Returns(genRepoMock.Object);
            var noteService = new NoteService(UnitOfWorkMock.Object);
            var itemsCountBefore = await genRepoMock.Object.GetItemsCount();
            //Arrange
            var handler = new CreateNoteCommandHandler(Mapper, noteService);

            var request = new CreateNoteCommand()
            {
                NoteDto = new NoteCreationDto()
                {
                    Value = 100,
                    Currency = Currency.USD.ToString(),
                    RoomId = Guid.NewGuid(),
                    UserId = Guid.NewGuid()
                }
            };

            //Act
            var result = await handler.Handle(request, CancellationToken.None);
            var itemsCountAter = await genRepoMock.Object.GetItemsCount();

            //Assert
            result.IsSuccess.ShouldBeTrue();
            itemsCountAter.ShouldBe(itemsCountBefore + 1);
        }
    }
}