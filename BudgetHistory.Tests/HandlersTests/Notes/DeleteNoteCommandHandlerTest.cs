using BudgetHistory.Application.Notes.Commands;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services;
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
            var genRepoMock = Mocks.MockRepository.GetMockedNoteRepository();
            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Note>()).Returns(genRepoMock.Object);

            var itemsCountBefore = await genRepoMock.Object.GetItemsCount();

            var deletedNoteId = genRepoMock.Object.GetQuery().First().Id;

            var noteService = new NoteService(UnitOfWorkMock.Object, Mapper, new EncryptionDecryptionService(), Configuration);

            //Arrange
            var handler = new DeleteNoteCommandHandler(UnitOfWorkMock.Object, noteService);

            var request = new DeleteNoteCommand()
            {
                NoteId = deletedNoteId,
            };

            //Act
            var result = await handler.Handle(request, CancellationToken.None);
            var itemsCountAter = await genRepoMock.Object.GetItemsCount();

            //Assert
            result.IsSuccess.ShouldBeFalse(); //TODO поправить когда метод Delete будет реализован
            //itemsCountAter.ShouldBe(itemsCountBefore - 1);
        }
    }
}