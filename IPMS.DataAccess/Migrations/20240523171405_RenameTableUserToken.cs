using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class RenameTableUserToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleToken_Account_UserId",
                table: "RoleToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleToken",
                table: "RoleToken");

            migrationBuilder.RenameTable(
                name: "RoleToken",
                newName: "UserToken");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserToken",
                table: "UserToken",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserToken_Account_UserId",
                table: "UserToken",
                column: "UserId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserToken_Account_UserId",
                table: "UserToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserToken",
                table: "UserToken");

            migrationBuilder.RenameTable(
                name: "UserToken",
                newName: "RoleToken");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleToken",
                table: "RoleToken",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_RoleToken_Account_UserId",
                table: "RoleToken",
                column: "UserId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
