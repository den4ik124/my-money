using Microsoft.EntityFrameworkCore.Migrations;

namespace BudgetHistory.Data.Migrations
{
    public partial class UserAndRoomsChanged : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Notes_RoomId",
                table: "Notes",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Rooms_RoomId",
                table: "Notes",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Rooms_RoomId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_RoomId",
                table: "Notes");
        }
    }
}
