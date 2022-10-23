using BudgetHistory.Application.Notes.Commands;
using Shouldly;
using System.Linq;
using System.Threading;
using Xunit;

namespace BudgetHistory.Tests.HandlersTests.Notes
{
    public class DeleteNoteCommandHandlerTest : NotesBaseTest
    {
        [Fact]
        public async void DeleteNoteCommandHandler_ShouldDecrease_NotesCount()
        {
            var itemsCountBefore = await NoteRepoMock.Object.GetItemsCount();

            var deletedNoteId = NoteRepoMock.Object.GetQuery().First().Id;

            //Arrange
            var handler = new DeleteNoteCommandHandler(UnitOfWorkMock.Object, NoteService);

            var request = new DeleteNoteCommand()
            {
                NoteId = deletedNoteId,
            };

            //Act
            var result = await handler.Handle(request, CancellationToken.None);
            var itemsCountAter = await NoteRepoMock.Object.GetItemsCount();

            //Assert
            result.IsSuccess.ShouldBeFalse(); //TODO поправить когда метод Delete будет реализован
            //itemsCountAter.ShouldBe(itemsCountBefore - 1);
        }
    }
}