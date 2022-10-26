using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Services;
using BudgetHistory.Core.Services.Interfaces;
using BudgetHistory.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetHistory.API.Extensions
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IEncryptionDecryption, EncryptionDecryptionService>();
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<IRoomService, RoomService>();

            return services;
        }
    }
}