using BudgetHistory.Application.DTOs;
using BudgetHistory.Application.Notes.Commands;
using BudgetHistory.Core.Models;
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

            var itemsCountBefore = await genRepoMock.Object.GetItemsCount();
            //Arrange
            var handler = new CreateNoteCommandHandler(UnitOfWorkMock.Object, Mapper);

            var request = new CreateNoteCommand()
            {
                NoteDto = new NoteDto()
                {
                    Value = 100,
                    Currency = Currency.USD,
                    DateOfCreation = DateTime.Now,
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