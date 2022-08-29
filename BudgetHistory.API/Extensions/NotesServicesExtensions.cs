using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notebook.Application.Notes.Queries;
using Notebook.Core.Interfaces.Repositories;
using Notebook.Data;
using Notebook.Data.Repositories;

namespace Notebook.API.Extensions
{
    public static class NotesServicesExtensions
    {
        public static IServiceCollection AddNotesServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<NotesDbContext>(opt =>
            {
                opt.UseSqlServer(config.GetConnectionString("Budget.History.Db"));
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            var context = services.BuildServiceProvider().GetService<NotesDbContext>();
            context.Database.Migrate();

            services.AddMediatR(typeof(GetNotesQueryHandler).Assembly);

            return services;
        }
    }
}