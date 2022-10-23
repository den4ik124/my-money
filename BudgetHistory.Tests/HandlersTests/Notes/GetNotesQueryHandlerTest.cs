using BudgetHistory.Application.DTOs.Common;
using BudgetHistory.Application.Notes.Queries;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Interfaces.Repositories;
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
    public class GetNotesQueryHandlerTest : NotesBaseTest
    {
        private readonly Mock<IGenericRepository<Room>> genRoomRepoMock;
        private readonly Mock<IGenericRepository<Note>> genNoteRepoMock;

        public GetNotesQueryHandlerTest()
        {
            genRoomRepoMock = Mocks.MockRepository.GetMockedRoomRepository();
            genNoteRepoMock = Mocks.MockRepository.GetMockedNoteRepository();
            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Note>()).Returns(genNoteRepoMock.Object);
            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Room>()).Returns(genRoomRepoMock.Object);

            genRoomRepoMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync((Guid id) =>
            {
                return genRoomRepoMock.Object.GetAll().GetAwaiter().GetResult().FirstOrDefault(item => item.Id == id);
            });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void GetNotesQueryHandler_ShouldReturn_ListOfMockedNotes(int size)
        {
            var encrDecrService = new EncryptionDecryptionService();
            var room = genRoomRepoMock.Object.GetQuery().FirstOrDefault();

            foreach (var note in genNoteRepoMock.Object.GetAll().Result)
            {
                note.EncryptedValue = encrDecrService.Encrypt(note.Value.ToString(), room.Password);
                note.EncryptedBalance = encrDecrService.Encrypt(note.Balance.ToString(), room.Password);
            }

            room.Password = encrDecrService.Encrypt(room.Password, Configuration.GetSection(AppSettings.SecretKey).Value);

            var noteService = new NoteService(UnitOfWorkMock.Object, Mapper, new EncryptionDecryptionService(), Configuration);
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