using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class ChangeShortNameClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "Name", newName: "ShortName", table: "IPMSClass");
            migrationBuilder.RenameColumn(name: "Description", newName: "Name", table: "IPMSClass");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "Name", newName: "Description", table: "IPMSClass");
            migrationBuilder.RenameColumn(name: "ShortName", newName: "Name", table: "IPMSClass");
        }
    }
}
