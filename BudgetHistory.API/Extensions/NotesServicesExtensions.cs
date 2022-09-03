using BudgetHistory.Application.Notes.Queries;
using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Services;
using BudgetHistory.Data;
using BudgetHistory.Data.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetHistory.API.Extensions
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
            services.AddSingleton<IEncryptionDecryption, EncryptionDecryptionService>();

            var context = services.BuildServiceProvider().GetService<NotesDbContext>();
            context.Database.Migrate();

            services.AddMediatR(typeof(GetNotesQueryHandler).Assembly);

            return services;
        }
    }
}