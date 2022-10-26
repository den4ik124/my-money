using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Data;
using BudgetHistory.Data.Seed.Interfaces;
using BudgetHistory.Logging.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace BudgetHistory.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args).Build();

            using var scope = builder.Services.CreateScope();

            var services = scope.ServiceProvider;
            var logger = services.GetService<ILogger<Program>>();
            var tgLogger = services.GetService<ITgLogger>();
            try
            {
                var dbContexts = new List<DbContext>()
                {
                    services.GetService<NotesDbContext>(),
                    services.GetService<UserDbContext>()
                };

                foreach (var context in dbContexts)
                {
                    context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured during Migration.\n" + ex.Message);
                tgLogger.LogError($"An error occured during Migration.\n{ex.Message}");
            }
            try
            {
                var seeder = services.GetRequiredService<ISeedEmployees>();
                var config = services.GetRequiredService<IConfiguration>();
                var uow = services.GetRequiredService<IUnitOfWork>();
                seeder.Seed(services, config, uow);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured during data seeding.\n" + ex.Message);
                tgLogger.LogError($"An error occured during data seeding.\n{ex.Message}");
            }

            builder.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}