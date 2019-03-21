using Microsoft.EntityFrameworkCore.Migrations;

namespace DiagnosticsApp.Migrations
{
    public partial class removeusernameaddphonenumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "username",
                table: "User",
                newName: "phoneNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Username",
                table: "User",
                newName: "IX_UserPhoneNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "phoneNumber",
                table: "User",
                newName: "username");

            migrationBuilder.RenameIndex(
                name: "IX_UserPhoneNumber",
                table: "User",
                newName: "IX_Username");
        }
    }
}
