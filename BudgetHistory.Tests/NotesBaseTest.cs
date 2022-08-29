using AutoMapper;
using Moq;
using Notebook.API.Mappings;
using Notebook.Core.Interfaces.Repositories;

namespace Notebook.Tests
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
        }

        public IMapper Mapper { get; private set; }

        public Mock<IUnitOfWork> UnitOfWorkMock;
    }
}