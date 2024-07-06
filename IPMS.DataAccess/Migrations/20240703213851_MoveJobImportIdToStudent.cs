using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class MoveJobImportIdToStudent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobImportId",
                table: "IPMSClass");

            migrationBuilder.AddColumn<int>(
                name: "JobImportId",
                table: "Student",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobImportId",
                table: "Student");

            migrationBuilder.AddColumn<int>(
                name: "JobImportId",
                table: "IPMSClass",
                type: "integer",
                nullable: true);
        }
    }
}
