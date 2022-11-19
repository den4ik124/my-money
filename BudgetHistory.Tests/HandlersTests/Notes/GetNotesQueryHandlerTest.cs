using BudgetHistory.Abstractions.Services;
using BudgetHistory.Application.DTOs.Common;
using BudgetHistory.Application.Notes.Queries;
using BudgetHistory.Business.Services;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Extensions;
using Moq;
using Shouldly;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BudgetHistory.Tests.HandlersTests.Notes
{
    public class GetNotesQueryHandlerTest : NotesBaseTest
    {
        public GetNotesQueryHandlerTest()
        {
            RoomRepoMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync((Guid id) =>
            {
                return RoomRepoMock.Object.GetAll().GetAwaiter().GetResult().FirstOrDefault(item => item.Id == id);
            });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetNotesQueryHandler_ShouldReturn_ListOfMockedNotes(int size)
        {
            var tokenServiceMock = new Mock<ITokenService>();

            var room = await RoomRepoMock.Object.GetQuery().FirstOrDefault().DecryptValues(EncryptionService, Configuration.GetSection(AppSettings.SecretKey).Value);
            var roomService = new RoomService(UnitOfWorkMock.Object, EncryptionService, Configuration, tokenServiceMock.Object, LoggerMock.Object);
            foreach (var note in NoteRepoMock.Object.GetAll().Result)
            {
                note.EncryptedValue = await EncryptionService.Encrypt(note.Value.ToString(), room.Password);
                note.EncryptedBalance = await EncryptionService.Encrypt(note.Balance.ToString(), room.Password);
            }

            var noteService = new NoteService(UnitOfWorkMock.Object, Mapper, roomService, EncryptionService, LoggerMock.Object);
            //Arrange
            var handler = new GetNotesQueryHandler(UnitOfWorkMock.Object, Mapper, noteService);
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
                RoomId = room.Id,
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