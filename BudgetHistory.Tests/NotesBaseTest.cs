using AutoMapper;
using BudgetHistory.API.Mappings;
using BudgetHistory.Core.Interfaces.Repositories;
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
        }

        public IMapper Mapper { get; private set; }
        public IConfiguration Configuration { get; }

        public Mock<IUnitOfWork> UnitOfWorkMock;
    }
}