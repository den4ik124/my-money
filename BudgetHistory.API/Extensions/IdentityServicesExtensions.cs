using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Notebook.Core.AppSettings;
using Notebook.Core.Constants;
using Notebook.Core.Services;
using Notebook.Data;
using Notebook.Data.Seed.Interfaces;
using Notebook.Data.Seed.Seeders;
using System.Text;

namespace Notebook.API.Extensions
{
    public static class IdentityServicesExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services,
            IConfiguration config)
        {
            services.AddDbContext<UserDbContext>(opt =>
            {
                opt.UseSqlServer(config.GetConnectionString("Budget.History.Users.Db"));
            });
            var context = services.BuildServiceProvider().GetService<UserDbContext>();
            context.Database.Migrate();

            services.AddIdentityCore<IdentityUser>(opt =>
            {
                opt.Password.RequireDigit = true;
                opt.Password.RequiredLength = 6;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireLowercase = false;
            })
                .AddRoles<IdentityRole>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddEntityFrameworkStores<UserDbContext>()
                .AddSignInManager<SignInManager<IdentityUser>>()
                .AddErrorDescriber<IdentityErrorDescriber>();

            services.Configure<AuthTokenParameters>(config.GetSection("Authentication:Token"));

            var authTokenParameters = new AuthTokenParameters();
            config.Bind("Authentication:Token", authTokenParameters);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authTokenParameters.SigningKey));

            services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
                {
                    opt.SaveToken = true;
                    opt.RequireHttpsMetadata = true;
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = true,
                        ValidIssuer = authTokenParameters.Issuer,
                        ValidateAudience = true,
                        ValidAudience = authTokenParameters.Audience,
                        //ValidateLifetime = true,
                        //ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy(nameof(Policies.AdminAccess), policy => policy.RequireRole(nameof(Roles.Admin)));
                opt.AddPolicy(nameof(Policies.ManagerAccess), policy => policy.RequireAssertion(context =>
                                        context.User.IsInRole(nameof(Roles.Admin)) ||
                                        context.User.IsInRole(nameof(Roles.Manager))));
                opt.AddPolicy(nameof(Policies.CustomerAccess), policy => policy.RequireAssertion(context =>
                                        context.User.IsInRole(nameof(Roles.Admin)) ||
                                        context.User.IsInRole(nameof(Roles.Manager)) ||
                                        context.User.IsInRole(nameof(Roles.Customer))));
                opt.AddPolicy(nameof(Policies.GuestAccess), policy => policy.RequireAssertion(context =>
                                        context.User.IsInRole(nameof(Roles.Admin)) ||
                                        context.User.IsInRole(nameof(Roles.Manager)) ||
                                        context.User.IsInRole(nameof(Roles.Customer)) ||
                                        context.User.IsInRole(nameof(Roles.Guest))));
            });

            services.AddTransient<ISeedEmployees, SeedEmployees>();

            services.AddScoped<TokenService>();

            return services;
        }
    }
}