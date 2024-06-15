using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class AddIndexUniqueForShortName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Syllabus_Id",
                table: "Syllabus",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Semester_ShortName",
                table: "Semester",
                column: "ShortName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IPMSClass_Name",
                table: "IPMSClass",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Syllabus_Id",
                table: "Syllabus");

            migrationBuilder.DropIndex(
                name: "IX_Semester_ShortName",
                table: "Semester");

            migrationBuilder.DropIndex(
                name: "IX_IPMSClass_Name",
                table: "IPMSClass");
        }
    }
}
