using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class Add_KitProject_Borrow_Return : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KitDevices_BasicIoTDevices_DeviceId",
                table: "KitDevices");

            migrationBuilder.DropForeignKey(
                name: "FK_KitDevices_IoTKits_KitId",
                table: "KitDevices");

            migrationBuilder.DropForeignKey(
                name: "FK_KitProjects_IoTKits_KitId",
                table: "KitProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_KitProjects_Project_ProjectId",
                table: "KitProjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KitProjects",
                table: "KitProjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KitDevices",
                table: "KitDevices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IoTKits",
                table: "IoTKits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BasicIoTDevices",
                table: "BasicIoTDevices");

            migrationBuilder.RenameTable(
                name: "KitProjects",
                newName: "KitProject");

            migrationBuilder.RenameTable(
                name: "KitDevices",
                newName: "KitDevice");

            migrationBuilder.RenameTable(
                name: "IoTKits",
                newName: "IoTKit");

            migrationBuilder.RenameTable(
                name: "BasicIoTDevices",
                newName: "BasicIoTDevice");

            migrationBuilder.RenameIndex(
                name: "IX_KitProjects_ProjectId",
                table: "KitProject",
                newName: "IX_KitProject_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_KitProjects_KitId",
                table: "KitProject",
                newName: "IX_KitProject_KitId");

            migrationBuilder.RenameIndex(
                name: "IX_KitDevices_KitId",
                table: "KitDevice",
                newName: "IX_KitDevice_KitId");

            migrationBuilder.RenameIndex(
                name: "IX_KitDevices_DeviceId",
                table: "KitDevice",
                newName: "IX_KitDevice_DeviceId");

            migrationBuilder.AddColumn<Guid>(
                name: "BorrowerId",
                table: "KitProject",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ReturnerId",
                table: "KitProject",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_KitProject",
                table: "KitProject",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KitDevice",
                table: "KitDevice",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IoTKit",
                table: "IoTKit",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BasicIoTDevice",
                table: "BasicIoTDevice",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_KitProject_BorrowerId",
                table: "KitProject",
                column: "BorrowerId");

            migrationBuilder.CreateIndex(
                name: "IX_KitProject_ReturnerId",
                table: "KitProject",
                column: "ReturnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_KitDevice_BasicIoTDevice_DeviceId",
                table: "KitDevice",
                column: "DeviceId",
                principalTable: "BasicIoTDevice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KitDevice_IoTKit_KitId",
                table: "KitDevice",
                column: "KitId",
                principalTable: "IoTKit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KitProject_IoTKit_KitId",
                table: "KitProject",
                column: "KitId",
                principalTable: "IoTKit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KitProject_Project_ProjectId",
                table: "KitProject",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KitProject_Student_BorrowerId",
                table: "KitProject",
                column: "BorrowerId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KitProject_Student_ReturnerId",
                table: "KitProject",
                column: "ReturnerId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KitDevice_BasicIoTDevice_DeviceId",
                table: "KitDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_KitDevice_IoTKit_KitId",
                table: "KitDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_KitProject_IoTKit_KitId",
                table: "KitProject");

            migrationBuilder.DropForeignKey(
                name: "FK_KitProject_Project_ProjectId",
                table: "KitProject");

            migrationBuilder.DropForeignKey(
                name: "FK_KitProject_Student_BorrowerId",
                table: "KitProject");

            migrationBuilder.DropForeignKey(
                name: "FK_KitProject_Student_ReturnerId",
                table: "KitProject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KitProject",
                table: "KitProject");

            migrationBuilder.DropIndex(
                name: "IX_KitProject_BorrowerId",
                table: "KitProject");

            migrationBuilder.DropIndex(
                name: "IX_KitProject_ReturnerId",
                table: "KitProject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KitDevice",
                table: "KitDevice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IoTKit",
                table: "IoTKit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BasicIoTDevice",
                table: "BasicIoTDevice");

            migrationBuilder.DropColumn(
                name: "BorrowerId",
                table: "KitProject");

            migrationBuilder.DropColumn(
                name: "ReturnerId",
                table: "KitProject");

            migrationBuilder.RenameTable(
                name: "KitProject",
                newName: "KitProjects");

            migrationBuilder.RenameTable(
                name: "KitDevice",
                newName: "KitDevices");

            migrationBuilder.RenameTable(
                name: "IoTKit",
                newName: "IoTKits");

            migrationBuilder.RenameTable(
                name: "BasicIoTDevice",
                newName: "BasicIoTDevices");

            migrationBuilder.RenameIndex(
                name: "IX_KitProject_ProjectId",
                table: "KitProjects",
                newName: "IX_KitProjects_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_KitProject_KitId",
                table: "KitProjects",
                newName: "IX_KitProjects_KitId");

            migrationBuilder.RenameIndex(
                name: "IX_KitDevice_KitId",
                table: "KitDevices",
                newName: "IX_KitDevices_KitId");

            migrationBuilder.RenameIndex(
                name: "IX_KitDevice_DeviceId",
                table: "KitDevices",
                newName: "IX_KitDevices_DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KitProjects",
                table: "KitProjects",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KitDevices",
                table: "KitDevices",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IoTKits",
                table: "IoTKits",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BasicIoTDevices",
                table: "BasicIoTDevices",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KitDevices_BasicIoTDevices_DeviceId",
                table: "KitDevices",
                column: "DeviceId",
                principalTable: "BasicIoTDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KitDevices_IoTKits_KitId",
                table: "KitDevices",
                column: "KitId",
                principalTable: "IoTKits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KitProjects_IoTKits_KitId",
                table: "KitProjects",
                column: "KitId",
                principalTable: "IoTKits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KitProjects_Project_ProjectId",
                table: "KitProjects",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
