using Azure.Identity;
using BudgetHistory.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetHistory.Data
{
    public class NotesDbContext : DbContext
    {
        public NotesDbContext(DbContextOptions<NotesDbContext> options) : base(options)
        {
            var sqlConnection = (Microsoft.Data.SqlClient.SqlConnection)Database.GetDbConnection();
            var credential = new DefaultAzureCredential();
            var token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }));
            sqlConnection.AccessToken = token.Token;
        }

        public DbSet<Note> Notes { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}