using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class SeperateModuleDeadline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "SubmissionModule");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "SubmissionModule");

            migrationBuilder.CreateTable(
                name: "ClassModuleDeadline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmissionModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassModuleDeadline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassModuleDeadline_IPMSClass_ClassId",
                        column: x => x.ClassId,
                        principalTable: "IPMSClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassModuleDeadline_SubmissionModule_SubmissionModuleId",
                        column: x => x.SubmissionModuleId,
                        principalTable: "SubmissionModule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassModuleDeadline_ClassId",
                table: "ClassModuleDeadline",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassModuleDeadline_SubmissionModuleId",
                table: "ClassModuleDeadline",
                column: "SubmissionModuleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassModuleDeadline");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "SubmissionModule",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "SubmissionModule",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
