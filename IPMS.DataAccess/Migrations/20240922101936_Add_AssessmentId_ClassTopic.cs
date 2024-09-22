using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class Add_AssessmentId_ClassTopic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassTopic_Project_ProjectId",
                table: "ClassTopic");

            migrationBuilder.DropIndex(
                name: "IX_ClassTopic_ProjectId",
                table: "ClassTopic");

            migrationBuilder.AddColumn<Guid>(
                name: "AssessmentId",
                table: "ClassTopic",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassTopic_AssessmentId",
                table: "ClassTopic",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTopic_ProjectId",
                table: "ClassTopic",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTopic_Assessment_AssessmentId",
                table: "ClassTopic",
                column: "AssessmentId",
                principalTable: "Assessment",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTopic_Project_ProjectId",
                table: "ClassTopic",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassTopic_Assessment_AssessmentId",
                table: "ClassTopic");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTopic_Project_ProjectId",
                table: "ClassTopic");

            migrationBuilder.DropIndex(
                name: "IX_ClassTopic_AssessmentId",
                table: "ClassTopic");

            migrationBuilder.DropIndex(
                name: "IX_ClassTopic_ProjectId",
                table: "ClassTopic");

            migrationBuilder.DropColumn(
                name: "AssessmentId",
                table: "ClassTopic");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTopic_ProjectId",
                table: "ClassTopic",
                column: "ProjectId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTopic_Project_ProjectId",
                table: "ClassTopic",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id");
        }
    }
}
