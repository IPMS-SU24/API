using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class ChangeFileLinkToFileName_AddCreateAtColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Report");

            migrationBuilder.RenameColumn(
                name: "FileLink",
                table: "Report",
                newName: "FileName");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ComponentsMaster",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ComponentsMaster");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Report",
                newName: "FileLink");

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Report",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
