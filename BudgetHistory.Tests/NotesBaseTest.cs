using AutoMapper;
using BudgetHistory.API.Mappings;
using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services;
using BudgetHistory.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IO;

namespace BudgetHistory.Tests
{
    public class NotesBaseTest
    {
        public NotesBaseTest()
        {
            var mapperConfig = new MapperConfiguration(c =>
            {
                c.AddProfile<ModelsMapper>();
            });
            Mapper = mapperConfig.CreateMapper();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            UnitOfWorkMock.Setup(x => x.CompleteAsync()).ReturnsAsync(true);
            Configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            EncryptionService = new EncryptionDecryptionService();

            var tokenServiceMock = new Mock<ITokenService>();

            RoomRepoMock = Mocks.MockRepository.GetMockedRoomRepository();
            NoteRepoMock = Mocks.MockRepository.GetMockedNoteRepository();

            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Note>()).Returns(NoteRepoMock.Object);
            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Room>()).Returns(RoomRepoMock.Object);

            RoomService = new RoomService(UnitOfWorkMock.Object, EncryptionService, Configuration, tokenServiceMock.Object);
            NoteService = new NoteService(UnitOfWorkMock.Object, Mapper, RoomService, EncryptionService);
        }

        public IRoomService RoomService { get; set; }
        public INoteService NoteService { get; set; }
        public IEncryptionDecryption EncryptionService { get; set; }
        public IMapper Mapper { get; private set; }
        public IConfiguration Configuration { get; }
        public Mock<IGenericRepository<Note>> NoteRepoMock { get; }
        public Mock<IGenericRepository<Room>> RoomRepoMock { get; }

        public Mock<IUnitOfWork> UnitOfWorkMock;
    }
}