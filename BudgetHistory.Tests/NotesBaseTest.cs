using AutoMapper;
using BudgetHistory.Abstractions.Interfaces;
using BudgetHistory.Abstractions.Interfaces.Data;
using BudgetHistory.Abstractions.Services;
using BudgetHistory.API.Mappings;
using BudgetHistory.Business.Services;
using BudgetHistory.Core.Models;
using BudgetHistory.Logging.Interfaces;
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
            LoggerMock = new Mock<ICustomLoggerFactory>();
            UnitOfWorkMock.Setup(x => x.CompleteAsync()).ReturnsAsync(true);
            Configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            EncryptionService = new EncryptionDecryptionService(LoggerMock.Object);

            var tokenServiceMock = new Mock<ITokenService>();

            RoomRepoMock = Mocks.MockRepository.GetMockedRoomRepository();
            NoteRepoMock = Mocks.MockRepository.GetMockedNoteRepository();

            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Note>()).Returns(NoteRepoMock.Object);
            UnitOfWorkMock.Setup(x => x.GetGenericRepository<Room>()).Returns(RoomRepoMock.Object);

            RoomService = new RoomService(UnitOfWorkMock.Object, EncryptionService, Configuration, tokenServiceMock.Object, LoggerMock.Object);
            NoteService = new NoteService(UnitOfWorkMock.Object, Mapper, RoomService, EncryptionService, LoggerMock.Object);
        }

        public IRoomService RoomService { get; set; }
        public INoteService NoteService { get; set; }
        public IEncryptionDecryption EncryptionService { get; set; }
        public IMapper Mapper { get; private set; }
        public IConfiguration Configuration { get; }
        public Mock<IGenericRepository<Note>> NoteRepoMock { get; }
        public Mock<IGenericRepository<Room>> RoomRepoMock { get; }

        public Mock<IUnitOfWork> UnitOfWorkMock;

        public Mock<ICustomLoggerFactory> LoggerMock { get; }
    }
}