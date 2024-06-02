using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class Update_ProjectSubmission_ChangeSubmissionLink_Name_AddSubmitter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmissionLink",
                table: "ProjectSubmission");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ProjectSubmission",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SubmitterId",
                table: "ProjectSubmission",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSubmission_SubmitterId",
                table: "ProjectSubmission",
                column: "SubmitterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubmission_Account_SubmitterId",
                table: "ProjectSubmission",
                column: "SubmitterId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubmission_Account_SubmitterId",
                table: "ProjectSubmission");

            migrationBuilder.DropIndex(
                name: "IX_ProjectSubmission_SubmitterId",
                table: "ProjectSubmission");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ProjectSubmission");

            migrationBuilder.DropColumn(
                name: "SubmitterId",
                table: "ProjectSubmission");

            migrationBuilder.AddColumn<string>(
                name: "SubmissionLink",
                table: "ProjectSubmission",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
