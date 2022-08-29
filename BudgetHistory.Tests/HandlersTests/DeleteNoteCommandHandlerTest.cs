using Notebook.Application.Notes.Commands;
using Notebook.Core;
using Shouldly;
using System;
using System.Threading;
using Xunit;

namespace Notebook.Tests.HandlersTests
{
    public class DeleteNoteCommandHandlerTest : NotesBaseTest
    {
        [Fact]
        public async void DeleteNoteCommandHandler_ShouldDecrease_NotesCount()
        {
            var genRepoMock = Mocks.MockRepository.GetMockedNoteRepository();
            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Note>()).Returns(genRepoMock.Object);

            var itemsCountBefore = await genRepoMock.Object.GetItemsCount();
            //Arrange
            var handler = new DeleteNoteCommandHandler(UnitOfWorkMock.Object);

            var request = new DeleteNoteCommand()
            {
                NoteId = Guid.NewGuid(),
            };

            //Act
            var result = await handler.Handle(request, CancellationToken.None);
            var itemsCountAter = await genRepoMock.Object.GetItemsCount();

            //Assert
            result.IsSuccess.ShouldBeTrue();
            itemsCountAter.ShouldBe(itemsCountBefore - 1);
        }
    }
}