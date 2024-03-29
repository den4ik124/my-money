using BudgetHistory.API.Extensions;
using BudgetHistory.API.Middleware;
using BudgetHistory.Application.Notes.Validators;
using BudgetHistory.Logging;
using BudgetHistory.Logging.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace BudgetHistory.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddNotesServices(Configuration);
            services.AddIdentityServices(Configuration);
            services.AddCustomServices();
            services.AddSingleton<ICustomLoggerFactory, CustomLoggerFactory>();

            services.AddAutoMapper(typeof(Startup));

            services.AddValidatorsFromAssemblyContaining<NoteCreationDtoValidator>()
                    .AddFluentValidationAutoValidation(c => c.DisableDataAnnotationsValidation = false);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notebook", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
                              IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notebook v1"));
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("CorsPolicy");

            app.UseMiddleware<AuthTokenCheckerMiddleware>();
            app.UseMiddleware<RoomTokenCheckerMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}