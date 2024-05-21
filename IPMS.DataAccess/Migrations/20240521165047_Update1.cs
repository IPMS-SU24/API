using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPMS.DataAccess.Migrations
{
    public partial class Update1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessment_Syllabus_SyllabusId",
                table: "Assessment");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTopic_IPMSClass_ClassId",
                table: "ClassTopic");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTopic_Topic_TopicId",
                table: "ClassTopic");

            migrationBuilder.DropForeignKey(
                name: "FK_Committee_AspNetUsers_LecturerId",
                table: "Committee");

            migrationBuilder.DropForeignKey(
                name: "FK_Committee_IPMSClass_ClassId",
                table: "Committee");

            migrationBuilder.DropForeignKey(
                name: "FK_ComponentsMaster_IoTComponent_ComponentId",
                table: "ComponentsMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ComponentsMaster_Project_ProjectId",
                table: "ComponentsMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ComponentsMaster_Topic_TopicId",
                table: "ComponentsMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_Favorite_AspNetUsers_LecturerId",
                table: "Favorite");

            migrationBuilder.DropForeignKey(
                name: "FK_IoTComponent_AspNetUsers_IPMSUserId",
                table: "IoTComponent");

            migrationBuilder.DropForeignKey(
                name: "FK_IPMSClass_Semester_SemesterId",
                table: "IPMSClass");

            migrationBuilder.DropForeignKey(
                name: "FK_LecturerGrade_Committee_CommitteeId",
                table: "LecturerGrade");

            migrationBuilder.DropForeignKey(
                name: "FK_LecturerGrade_ProjectSubmission_SubmissionId",
                table: "LecturerGrade");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_AspNetUsers_OwnerId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubmission_Project_ProjectId",
                table: "ProjectSubmission");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubmission_SubmissionModule_SubmissionModuleId",
                table: "ProjectSubmission");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_AspNetUsers_ReporterId",
                table: "Report");

            migrationBuilder.DropForeignKey(
                name: "FK_Semester_Syllabus_SyllabusId",
                table: "Semester");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_AspNetUsers_InformationId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_IPMSClass_ClassId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Project_ProjectId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionModule_AspNetUsers_LectureId",
                table: "SubmissionModule");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionModule_Assessment_AssessmentId",
                table: "SubmissionModule");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionModule_Semester_SemesterId",
                table: "SubmissionModule");

            migrationBuilder.DropForeignKey(
                name: "FK_Topic_AspNetUsers_OwnerId",
                table: "Topic");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicFavorite_Favorite_FavoriteId",
                table: "TopicFavorite");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicFavorite_Topic_TopicId",
                table: "TopicFavorite");

            migrationBuilder.DropIndex(
                name: "IX_IoTComponent_IPMSUserId",
                table: "IoTComponent");

            migrationBuilder.DropIndex(
                name: "IX_ComponentsMaster_ProjectId",
                table: "ComponentsMaster");

            migrationBuilder.DropIndex(
                name: "IX_ComponentsMaster_TopicId",
                table: "ComponentsMaster");

            migrationBuilder.DropColumn(
                name: "IPMSUserId",
                table: "IoTComponent");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ComponentsMaster");

            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "ComponentsMaster");

            migrationBuilder.AddColumn<string>(
                name: "FileLink",
                table: "Report",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReportTypeId",
                table: "Report",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FinalGrade",
                table: "ProjectSubmission",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "Grade",
                table: "Project",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReporterId",
                table: "MemberHistory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<decimal>(
                name: "Grade",
                table: "LecturerGrade",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "ComponentsMaster",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "ReportType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Report_ReportTypeId",
                table: "Report",
                column: "ReportTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessment_Syllabus_SyllabusId",
                table: "Assessment",
                column: "SyllabusId",
                principalTable: "Syllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTopic_IPMSClass_ClassId",
                table: "ClassTopic",
                column: "ClassId",
                principalTable: "IPMSClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTopic_Topic_TopicId",
                table: "ClassTopic",
                column: "TopicId",
                principalTable: "Topic",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Committee_AspNetUsers_LecturerId",
                table: "Committee",
                column: "LecturerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Committee_IPMSClass_ClassId",
                table: "Committee",
                column: "ClassId",
                principalTable: "IPMSClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentsMaster_IoTComponent_ComponentId",
                table: "ComponentsMaster",
                column: "ComponentId",
                principalTable: "IoTComponent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Favorite_AspNetUsers_LecturerId",
                table: "Favorite",
                column: "LecturerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IPMSClass_Semester_SemesterId",
                table: "IPMSClass",
                column: "SemesterId",
                principalTable: "Semester",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LecturerGrade_Committee_CommitteeId",
                table: "LecturerGrade",
                column: "CommitteeId",
                principalTable: "Committee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LecturerGrade_ProjectSubmission_SubmissionId",
                table: "LecturerGrade",
                column: "SubmissionId",
                principalTable: "ProjectSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_AspNetUsers_OwnerId",
                table: "Project",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubmission_Project_ProjectId",
                table: "ProjectSubmission",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubmission_SubmissionModule_SubmissionModuleId",
                table: "ProjectSubmission",
                column: "SubmissionModuleId",
                principalTable: "SubmissionModule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Report_AspNetUsers_ReporterId",
                table: "Report",
                column: "ReporterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Report_ReportType_ReportTypeId",
                table: "Report",
                column: "ReportTypeId",
                principalTable: "ReportType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Semester_Syllabus_SyllabusId",
                table: "Semester",
                column: "SyllabusId",
                principalTable: "Syllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_AspNetUsers_InformationId",
                table: "Student",
                column: "InformationId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_IPMSClass_ClassId",
                table: "Student",
                column: "ClassId",
                principalTable: "IPMSClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Project_ProjectId",
                table: "Student",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionModule_AspNetUsers_LectureId",
                table: "SubmissionModule",
                column: "LectureId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionModule_Assessment_AssessmentId",
                table: "SubmissionModule",
                column: "AssessmentId",
                principalTable: "Assessment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionModule_Semester_SemesterId",
                table: "SubmissionModule",
                column: "SemesterId",
                principalTable: "Semester",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Topic_AspNetUsers_OwnerId",
                table: "Topic",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicFavorite_Favorite_FavoriteId",
                table: "TopicFavorite",
                column: "FavoriteId",
                principalTable: "Favorite",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicFavorite_Topic_TopicId",
                table: "TopicFavorite",
                column: "TopicId",
                principalTable: "Topic",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessment_Syllabus_SyllabusId",
                table: "Assessment");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTopic_IPMSClass_ClassId",
                table: "ClassTopic");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTopic_Topic_TopicId",
                table: "ClassTopic");

            migrationBuilder.DropForeignKey(
                name: "FK_Committee_AspNetUsers_LecturerId",
                table: "Committee");

            migrationBuilder.DropForeignKey(
                name: "FK_Committee_IPMSClass_ClassId",
                table: "Committee");

            migrationBuilder.DropForeignKey(
                name: "FK_ComponentsMaster_IoTComponent_ComponentId",
                table: "ComponentsMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_Favorite_AspNetUsers_LecturerId",
                table: "Favorite");

            migrationBuilder.DropForeignKey(
                name: "FK_IPMSClass_Semester_SemesterId",
                table: "IPMSClass");

            migrationBuilder.DropForeignKey(
                name: "FK_LecturerGrade_Committee_CommitteeId",
                table: "LecturerGrade");

            migrationBuilder.DropForeignKey(
                name: "FK_LecturerGrade_ProjectSubmission_SubmissionId",
                table: "LecturerGrade");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_AspNetUsers_OwnerId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubmission_Project_ProjectId",
                table: "ProjectSubmission");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubmission_SubmissionModule_SubmissionModuleId",
                table: "ProjectSubmission");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_AspNetUsers_ReporterId",
                table: "Report");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_ReportType_ReportTypeId",
                table: "Report");

            migrationBuilder.DropForeignKey(
                name: "FK_Semester_Syllabus_SyllabusId",
                table: "Semester");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_AspNetUsers_InformationId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_IPMSClass_ClassId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Project_ProjectId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionModule_AspNetUsers_LectureId",
                table: "SubmissionModule");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionModule_Assessment_AssessmentId",
                table: "SubmissionModule");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionModule_Semester_SemesterId",
                table: "SubmissionModule");

            migrationBuilder.DropForeignKey(
                name: "FK_Topic_AspNetUsers_OwnerId",
                table: "Topic");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicFavorite_Favorite_FavoriteId",
                table: "TopicFavorite");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicFavorite_Topic_TopicId",
                table: "TopicFavorite");

            migrationBuilder.DropTable(
                name: "ReportType");

            migrationBuilder.DropIndex(
                name: "IX_Report_ReportTypeId",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "FileLink",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "ReportTypeId",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "IsMemberSwapApproved",
                table: "MemberHistory");

            migrationBuilder.AlterColumn<decimal>(
                name: "FinalGrade",
                table: "ProjectSubmission",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Grade",
                table: "Project",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ReporterId",
                table: "MemberHistory",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectFromId",
                table: "MemberHistory",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<decimal>(
                name: "Grade",
                table: "LecturerGrade",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IPMSUserId",
                table: "IoTComponent",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "ComponentsMaster",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "ComponentsMaster",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TopicId",
                table: "ComponentsMaster",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IoTComponent_IPMSUserId",
                table: "IoTComponent",
                column: "IPMSUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentsMaster_ProjectId",
                table: "ComponentsMaster",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentsMaster_TopicId",
                table: "ComponentsMaster",
                column: "TopicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessment_Syllabus_SyllabusId",
                table: "Assessment",
                column: "SyllabusId",
                principalTable: "Syllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTopic_IPMSClass_ClassId",
                table: "ClassTopic",
                column: "ClassId",
                principalTable: "IPMSClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTopic_Topic_TopicId",
                table: "ClassTopic",
                column: "TopicId",
                principalTable: "Topic",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Committee_AspNetUsers_LecturerId",
                table: "Committee",
                column: "LecturerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Committee_IPMSClass_ClassId",
                table: "Committee",
                column: "ClassId",
                principalTable: "IPMSClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentsMaster_IoTComponent_ComponentId",
                table: "ComponentsMaster",
                column: "ComponentId",
                principalTable: "IoTComponent",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentsMaster_Project_ProjectId",
                table: "ComponentsMaster",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentsMaster_Topic_TopicId",
                table: "ComponentsMaster",
                column: "TopicId",
                principalTable: "Topic",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorite_AspNetUsers_LecturerId",
                table: "Favorite",
                column: "LecturerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_IoTComponent_AspNetUsers_IPMSUserId",
                table: "IoTComponent",
                column: "IPMSUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IPMSClass_Semester_SemesterId",
                table: "IPMSClass",
                column: "SemesterId",
                principalTable: "Semester",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LecturerGrade_Committee_CommitteeId",
                table: "LecturerGrade",
                column: "CommitteeId",
                principalTable: "Committee",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LecturerGrade_ProjectSubmission_SubmissionId",
                table: "LecturerGrade",
                column: "SubmissionId",
                principalTable: "ProjectSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_AspNetUsers_OwnerId",
                table: "Project",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubmission_Project_ProjectId",
                table: "ProjectSubmission",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubmission_SubmissionModule_SubmissionModuleId",
                table: "ProjectSubmission",
                column: "SubmissionModuleId",
                principalTable: "SubmissionModule",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Report_AspNetUsers_ReporterId",
                table: "Report",
                column: "ReporterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Semester_Syllabus_SyllabusId",
                table: "Semester",
                column: "SyllabusId",
                principalTable: "Syllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_AspNetUsers_InformationId",
                table: "Student",
                column: "InformationId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_IPMSClass_ClassId",
                table: "Student",
                column: "ClassId",
                principalTable: "IPMSClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Project_ProjectId",
                table: "Student",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionModule_AspNetUsers_LectureId",
                table: "SubmissionModule",
                column: "LectureId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionModule_Assessment_AssessmentId",
                table: "SubmissionModule",
                column: "AssessmentId",
                principalTable: "Assessment",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionModule_Semester_SemesterId",
                table: "SubmissionModule",
                column: "SemesterId",
                principalTable: "Semester",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Topic_AspNetUsers_OwnerId",
                table: "Topic",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicFavorite_Favorite_FavoriteId",
                table: "TopicFavorite",
                column: "FavoriteId",
                principalTable: "Favorite",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicFavorite_Topic_TopicId",
                table: "TopicFavorite",
                column: "TopicId",
                principalTable: "Topic",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
