using BudgetHistory.API.Policy.Handlers;
using BudgetHistory.Application.Notes.Queries;
using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Services;
using BudgetHistory.Core.Services.Interfaces;
using BudgetHistory.Data;
using BudgetHistory.Data.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetHistory.API.Extensions
{
    public static class NotesServicesExtensions
    {
        public static IServiceCollection AddNotesServices(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("Budget.History.Db");
            services.AddDbContext<NotesDbContext>(opt =>
            {
                opt.UseSqlServer(connectionString);
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IEncryptionDecryption, EncryptionDecryptionService>();
            services.AddTransient<INoteService, NoteService>();

            var context = services.BuildServiceProvider().GetService<NotesDbContext>();
            context.Database.Migrate();

            services.AddMediatR(typeof(GetNotesQueryHandler).Assembly);

            services.AddTransient<IAuthorizationHandler, RoomLoggedInHandler>();

            return services;
        }
    }
}