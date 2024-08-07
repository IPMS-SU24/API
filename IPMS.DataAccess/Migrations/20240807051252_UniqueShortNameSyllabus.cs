using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class UniqueShortNameSyllabus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Syllabus_ShortName",
                table: "Syllabus",
                column: "ShortName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Syllabus_ShortName",
                table: "Syllabus");
        }
    }
}
