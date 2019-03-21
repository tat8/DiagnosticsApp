using Microsoft.EntityFrameworkCore.Migrations;

namespace DiagnosticsApp.Migrations
{
    public partial class addusername : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "User",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Username",
                table: "User",
                column: "username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Username",
                table: "User");

            migrationBuilder.DropColumn(
                name: "username",
                table: "User");
        }
    }
}
