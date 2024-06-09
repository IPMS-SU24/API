using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class AddMaxMemberInGroupClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxMember",
                table: "IPMSClass",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxMember",
                table: "IPMSClass");
        }
    }
}
