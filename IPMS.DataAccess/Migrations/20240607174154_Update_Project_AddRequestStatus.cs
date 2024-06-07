using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class Update_Project_AddRequestStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMemberSwapApproved",
                table: "MemberHistory");

            migrationBuilder.DropColumn(
                name: "IsProjectFromApproved",
                table: "MemberHistory");

            migrationBuilder.DropColumn(
                name: "IsProjectToApproved",
                table: "MemberHistory");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Topic",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectFromId",
                table: "MemberHistory",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "MemberSwapStatus",
                table: "MemberHistory",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectFromStatus",
                table: "MemberHistory",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectToStatus",
                table: "MemberHistory",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Topic");

            migrationBuilder.DropColumn(
                name: "MemberSwapStatus",
                table: "MemberHistory");

            migrationBuilder.DropColumn(
                name: "ProjectFromStatus",
                table: "MemberHistory");

            migrationBuilder.DropColumn(
                name: "ProjectToStatus",
                table: "MemberHistory");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectFromId",
                table: "MemberHistory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMemberSwapApproved",
                table: "MemberHistory",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProjectFromApproved",
                table: "MemberHistory",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProjectToApproved",
                table: "MemberHistory",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
