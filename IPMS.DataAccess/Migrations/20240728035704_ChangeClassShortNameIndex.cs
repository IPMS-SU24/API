using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class ChangeClassShortNameIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IPMSClass_Name",
                table: "IPMSClass");

            migrationBuilder.CreateIndex(
                name: "IX_IPMSClass_ShortName_SemesterId",
                table: "IPMSClass",
                columns: new[] { "ShortName", "SemesterId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IPMSClass_ShortName_SemesterId",
                table: "IPMSClass");

            migrationBuilder.CreateIndex(
                name: "IX_IPMSClass_Name",
                table: "IPMSClass",
                column: "ShortName",
                unique: true);
        }
    }
}
