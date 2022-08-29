using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notebook.Core.Constants;
using Notebook.Data.Seed.Interfaces;
using System;

namespace Notebook.Data.Seed.Seeders
{
    public class SeedEmployees : ISeedEmployees
    {
        public void Seed(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            string login = "admin";
            string email = "admin@admin.com";

            var roles = Enum.GetNames<Roles>();

            foreach (var role in roles)
            {
                if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                {
                    roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                }
            }

            var administrator = new IdentityUser();
            administrator.Email = email;
            administrator.UserName = login;

            var adminUser = userManager.CreateAsync(administrator, "admin123").GetAwaiter().GetResult();

            if (adminUser.Succeeded)
            {
                SetRolesToUser(userManager, roles, administrator);
            }
        }

        private static void SetRolesToUser(UserManager<IdentityUser> userManager, string[] roles, IdentityUser testUser)
        {
            foreach (var role in roles)
            {
                if (!userManager.IsInRoleAsync(testUser, role).GetAwaiter().GetResult())
                {
                    userManager.AddToRoleAsync(testUser, role).GetAwaiter().GetResult();
                }
            }
        }
    }
}