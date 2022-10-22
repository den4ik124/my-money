using Microsoft.EntityFrameworkCore.Migrations;

namespace BudgetHistory.Data.Migrations
{
    public partial class NoteEntityModifiedEncryptedValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Notes");

            migrationBuilder.AddColumn<string>(
                name: "EncryptedBalance",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptedValue",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedBalance",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "EncryptedValue",
                table: "Notes");

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Notes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "Notes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
