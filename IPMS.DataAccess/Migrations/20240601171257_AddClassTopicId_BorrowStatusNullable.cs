using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class AddClassTopicId_BorrowStatusNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassTopicId",
                table: "Project",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ComponentsMaster",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassTopicId",
                table: "Project");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ComponentsMaster",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
