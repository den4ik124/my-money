using Notebook.Application.DTOs.Common;
using Notebook.Application.Notes.Queries;
using Notebook.Core;
using Shouldly;
using System.Linq;
using System.Threading;
using Xunit;

namespace Notebook.Tests.HandlersTests
{
    public class GetNotesQueryHandlerTest : NotesBaseTest
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void GetNotesQueryHandler_ShouldReturn_ListOfMockedNotes(int size)
        {
            var genRepoMock = Mocks.MockRepository.GetMockedNoteRepository();
            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Note>()).Returns(genRepoMock.Object);

            //Arrange
            var handler = new GetNotesQueryHandler(base.UnitOfWorkMock.Object, base.Mapper);
            var pageParameters = new PagingFilteringDto()
            {
                PageInfo = new Application.Core.PageInfo()
                {
                    Page = 1,
                    Size = size
                }
            };

            var request = new GetNotesQuery()
            {
                PageParameters = pageParameters.PageInfo
            };

            //Act
            var result = await handler.Handle(request, CancellationToken.None);

            //Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Items.Count().ShouldBe(size);
        }
    }
}