using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class Update_Topic_AddSuggester_Report_AddStatus_AddCreatedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SuggesterId",
                table: "Topic",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateAt",
                table: "Report",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Report",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Topic_SuggesterId",
                table: "Topic",
                column: "SuggesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Topic_Project_SuggesterId",
                table: "Topic",
                column: "SuggesterId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topic_Project_SuggesterId",
                table: "Topic");

            migrationBuilder.DropIndex(
                name: "IX_Topic_SuggesterId",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "SuggesterId",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "CreateAt",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Report");
        }
    }
}
