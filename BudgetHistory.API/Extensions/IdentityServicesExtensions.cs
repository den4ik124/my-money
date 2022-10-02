using BudgetHistory.API.Policy;
using BudgetHistory.Auth;
using BudgetHistory.Auth.Interfaces;
using BudgetHistory.Core.AppSettings;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Services;
using BudgetHistory.Data;
using BudgetHistory.Data.Seed.Interfaces;
using BudgetHistory.Data.Seed.Seeders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BudgetHistory.API.Extensions
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
                //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
                })
                .AddJwtBearer("RoomAuth", opt =>
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
            //.AddPolicyScheme("ServiceAuth_RoomAuth", "ServiceAuth_RoomAuth", opt =>
            //{
            //    opt.ForwardDefaultSelector = context =>
            //    {
            //        string roomAuthorization = context.Request.Cookies[Cookies.RoomAuth];
            //        if (!string.IsNullOrEmpty(roomAuthorization) && roomAuthorization.StartsWith("RoomAuth "))
            //        {
            //            return "RoomAuth";
            //        }
            //        return JwtBearerDefaults.AuthenticationScheme;
            //    };
            //});

            services.AddAuthorization(opt =>
            {
                //var defaultAuthPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme, "RoomAuth");
                //defaultAuthPolicyBuilder = defaultAuthPolicyBuilder.RequireAuthenticatedUser();
                //opt.DefaultPolicy = defaultAuthPolicyBuilder.Build();

                opt.AddPolicy(nameof(Policies.RoomLoggedIn), policy =>
                        {
                            policy.AuthenticationSchemes.Add("RoomAuth");
                            policy.RequireAuthenticatedUser();
                            policy.Requirements.Add(new RoomLoggedInPolicy());
                        });

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
            services.AddTransient<IAuthService, AuthService>();

            services.AddScoped<TokenService>();

            return services;
        }
    }
}