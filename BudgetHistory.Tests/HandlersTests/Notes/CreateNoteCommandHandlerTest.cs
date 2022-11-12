using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Application.Notes.Commands;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Extensions;
using BudgetHistory.Core.Models;
using Moq;
using Shouldly;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BudgetHistory.Tests.HandlersTests.Notes
{
    public class CreateNoteCommandHandlerTest : NotesBaseTest
    {
        [Fact]
        public async Task CreateNoteCommandHandler_ShouldIncrease_NotesCount()
        {
            RoomRepoMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync((Guid id) =>
            {
                return RoomRepoMock.Object.GetAll().GetAwaiter().GetResult().FirstOrDefault(item => item.Id == id);
            });

            var itemsCountBefore = await NoteRepoMock.Object.GetItemsCount();

            var room = RoomRepoMock.Object.GetQuery().FirstOrDefault();
            await room.DecryptValues(EncryptionService, Configuration.GetSection(AppSettings.SecretKey).Value);

            //Arrange
            var handler = new CreateNoteCommandHandler(Mapper, NoteService);

            var request = new CreateNoteCommand()
            {
                NoteDto = new NoteCreationDto()
                {
                    Value = 100,
                    Currency = Currency.USD.ToString(),
                    RoomId = room.Id,
                    UserId = Guid.NewGuid()
                }
            };

            //Act
            var result = await handler.Handle(request, CancellationToken.None);
            var itemsCountAter = await NoteRepoMock.Object.GetItemsCount();

            //Assert
            result.IsSuccess.ShouldBeTrue();
            itemsCountAter.ShouldBe(itemsCountBefore + 1);
        }
    }
}