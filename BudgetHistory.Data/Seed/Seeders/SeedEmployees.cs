using BudgetHistory.Abstractions.Interfaces.Data;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Models;
using BudgetHistory.Data.Seed.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace BudgetHistory.Data.Seed.Seeders
{
    public class SeedEmployees : ISeedEmployees
    {
        public void Seed(IServiceProvider serviceProvider, IConfiguration configuration, IUnitOfWork unitOfWork)
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
            var adminId = Guid.NewGuid();
            administrator.Id = adminId.ToString();
            administrator.Email = email;
            administrator.UserName = login;

            var adminUser = userManager.CreateAsync(administrator, "admin123").GetAwaiter().GetResult();

            if (adminUser.Succeeded)
            {
                SetRolesToUser(userManager, roles, administrator);
                var user = new User()
                {
                    Id = Guid.NewGuid(),
                    Email = administrator.Email,
                    UserName = login,
                    AssociatedIdentityUserId = adminId,
                    Rooms = new List<Room>()
                };

                var result = unitOfWork.GetGenericRepository<User>().Add(user).Result;
                if (result)
                {
                    result = unitOfWork.CompleteAsync().Result;
                }
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