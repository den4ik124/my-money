using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetHistory.Data
{
    public class UserDbContext : IdentityDbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
            //var sqlConnection = (Microsoft.Data.SqlClient.SqlConnection)Database.GetDbConnection();
            //var credential = new DefaultAzureCredential();
            //var token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }));
            //sqlConnection.AccessToken = token.Token;
        }

        public DbSet<IdentityUser> CustomUsers { get; set; }
    }
}