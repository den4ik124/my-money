using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Application.Notes.Commands;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services;
using Moq;
using Shouldly;
using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace BudgetHistory.Tests.HandlersTests.Notes
{
    public class CreateNoteCommandHandlerTest : NotesBaseTest
    {
        [Fact]
        public async void CreateNoteCommandHandler_ShouldIncrease_NotesCount()
        {
            var genNoteRepoMock = Mocks.MockRepository.GetMockedNoteRepository();
            var genRoomRepoMock = Mocks.MockRepository.GetMockedRoomRepository();

            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Note>()).Returns(genNoteRepoMock.Object);
            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Room>()).Returns(genRoomRepoMock.Object);
            genRoomRepoMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync((Guid id) =>
            {
                return genRoomRepoMock.Object.GetAll().GetAwaiter().GetResult().FirstOrDefault(item => item.Id == id);
            });

            var noteService = new NoteService(UnitOfWorkMock.Object, Mapper, new EncryptionDecryptionService(), Configuration);
            var itemsCountBefore = await genNoteRepoMock.Object.GetItemsCount();
            var encDecrService = new EncryptionDecryptionService();

            var room = genRoomRepoMock.Object.GetQuery().FirstOrDefault();
            room.Password = encDecrService.Encrypt(room.Password, Configuration.GetSection(AppSettings.SecretKey).Value);

            //Arrange
            var handler = new CreateNoteCommandHandler(Mapper, noteService, encDecrService, Configuration, UnitOfWorkMock.Object);

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
            var itemsCountAter = await genNoteRepoMock.Object.GetItemsCount();

            //Assert
            result.IsSuccess.ShouldBeTrue();
            itemsCountAter.ShouldBe(itemsCountBefore + 1);
        }
    }
}