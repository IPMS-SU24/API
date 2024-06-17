using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class AddIndexUniqueStudent_AuditingInterceptor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Student_ClassId",
                table: "Student");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserRefreshToken",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "UserRefreshToken",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TopicFavorite",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "TopicFavorite",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Topic",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Topic",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Syllabus",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Syllabus",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SubmissionModule",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "SubmissionModule",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Student",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Student",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Semester",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Semester",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ReportType",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "ReportType",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Report",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Report",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ProjectSubmission",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "ProjectSubmission",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Project",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Project",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MemberHistory",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "MemberHistory",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "LecturerGrade",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "LecturerGrade",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "IPMSClass",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "IPMSClass",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "IoTComponent",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "IoTComponent",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Favorite",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Favorite",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "ComponentsMaster",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Committee",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Committee",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ClassTopic",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "ClassTopic",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Assessment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Assessment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Student_ClassId_InformationId",
                table: "Student",
                columns: new[] { "ClassId", "InformationId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Student_ClassId_InformationId",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserRefreshToken");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "UserRefreshToken");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TopicFavorite");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "TopicFavorite");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Syllabus");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Syllabus");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SubmissionModule");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "SubmissionModule");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Semester");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Semester");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ReportType");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "ReportType");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ProjectSubmission");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "ProjectSubmission");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MemberHistory");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "MemberHistory");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "LecturerGrade");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "LecturerGrade");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "IPMSClass");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "IPMSClass");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "IoTComponent");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "IoTComponent");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Favorite");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Favorite");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "ComponentsMaster");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Committee");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Committee");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ClassTopic");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "ClassTopic");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Assessment");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Assessment");

            migrationBuilder.CreateIndex(
                name: "IX_Student_ClassId",
                table: "Student",
                column: "ClassId");
        }
    }
}
