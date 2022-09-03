using AutoMapper;
using BudgetHistory.API.Mappings;
using BudgetHistory.Core.Interfaces.Repositories;
using Moq;

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
        }

        public IMapper Mapper { get; private set; }

        public Mock<IUnitOfWork> UnitOfWorkMock;
    }
}