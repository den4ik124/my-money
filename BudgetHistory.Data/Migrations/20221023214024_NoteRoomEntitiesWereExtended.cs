using Microsoft.EntityFrameworkCore.Migrations;

namespace BudgetHistory.Data.Migrations
{
    public partial class NoteRoomEntitiesWereExtended : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Rooms",
                newName: "EncryptedPassword");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Notes");

            migrationBuilder.RenameColumn(
                name: "EncryptedPassword",
                table: "Rooms",
                newName: "Password");
        }
    }
}
