using BudgetHistory.Abstractions.Interfaces;
using BudgetHistory.Abstractions.Interfaces.Data;
using BudgetHistory.Abstractions.Interfaces.Services;
using BudgetHistory.Abstractions.Services;
using BudgetHistory.Business.Services;
using BudgetHistory.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetHistory.API.Extensions
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddSingleton<IEncryptionDecryption, EncryptionDecryptionService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}