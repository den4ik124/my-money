using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Application.Notes.Commands;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Extensions;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace BudgetHistory.Tests.HandlersTests.Notes
{
    public class EditNoteCommandHandlerTest : NotesBaseTest
    {
        [Fact]
        public async void EditNoteCommandHandler_ShouldUpdate_NoteData()
        {
            var genNoteRepoMock = Mocks.MockRepository.GetMockedNoteRepository();
            var genRoomRepoMock = Mocks.MockRepository.GetMockedRoomRepository();
            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Note>()).Returns(genNoteRepoMock.Object);
            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Room>()).Returns(genRoomRepoMock.Object);

            var encDecrService = new EncryptionDecryptionService();

            var room = genRoomRepoMock.Object.GetQuery().FirstOrDefault();

            var items = new List<Note>()
            {
                new Note()
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.Id,
                },
                new Note()
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.Id,
                },
                new Note()
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.Id,
                },
            };
            foreach (var item in items)
            {
                item.EncryptValues(encDecrService, room.Password);
            }

            room.Password = encDecrService.Encrypt(room.Password, Configuration.GetSection(AppSettings.SecretKey).Value);

            genNoteRepoMock.Setup(rep => rep.Update(It.IsAny<Note>())).Returns((Note updatedItem) =>
            {
                var item = items.FirstOrDefault();
                Mapper.Map(updatedItem, item);
                return true;
            });
            genNoteRepoMock.Setup(rep => rep.GetById(It.IsAny<Guid>())).ReturnsAsync(items.FirstOrDefault());

            genRoomRepoMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync((Guid id) =>
            {
                return genRoomRepoMock.Object.GetAll().GetAwaiter().GetResult().FirstOrDefault(item => item.Id == id);
            });

            var editedNote = genNoteRepoMock.Object.GetQuery().FirstOrDefault();
            var checkString = room.Id;

            var editedNoteDto = new NoteDto();
            Mapper.Map(editedNote, editedNoteDto);
            editedNoteDto.UserId = checkString;
            editedNoteDto.RoomId = checkString;

            var noteService = new NoteService(UnitOfWorkMock.Object, Mapper, encDecrService, Configuration);

            //Arrange
            var handler = new EditNoteCommandHandler(noteService, Mapper);

            var request = new EditNoteCommand()
            {
                EditedNote = editedNoteDto,
            };

            //Act
            var result = await handler.Handle(request, CancellationToken.None);

            //Assert
            var firstItem = items.FirstOrDefault();
            result.IsSuccess.ShouldBeTrue();
            firstItem.UserId.ShouldBe(checkString);
            firstItem.RoomId.ShouldBe(checkString);
        }
    }
}