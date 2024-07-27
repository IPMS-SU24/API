using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class ChangeProjectDeleteBehaviour : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topic_Project_SuggesterId",
                table: "Topic");

            migrationBuilder.AddForeignKey(
                name: "FK_Topic_Project_SuggesterId",
                table: "Topic",
                column: "SuggesterId",
                principalTable: "Project",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topic_Project_SuggesterId",
                table: "Topic");

            migrationBuilder.AddForeignKey(
                name: "FK_Topic_Project_SuggesterId",
                table: "Topic",
                column: "SuggesterId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
