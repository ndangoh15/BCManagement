using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateDocumentIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ExamCode",
                table: "CandidateDocuments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CentreCode",
                table: "CandidateDocuments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CandidateNumber",
                table: "CandidateDocuments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CandidateName",
                table: "CandidateDocuments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateDocuments_Session_Exam_CandidateName",
                table: "CandidateDocuments",
                columns: new[] { "Session", "ExamCode", "CandidateName" });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateDocuments_Session_Exam_CandidateNumber",
                table: "CandidateDocuments",
                columns: new[] { "Session", "ExamCode", "CandidateNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateDocuments_Session_Exam_Centre",
                table: "CandidateDocuments",
                columns: new[] { "Session", "ExamCode", "CentreCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CandidateDocuments_Session_Exam_CandidateName",
                table: "CandidateDocuments");

            migrationBuilder.DropIndex(
                name: "IX_CandidateDocuments_Session_Exam_CandidateNumber",
                table: "CandidateDocuments");

            migrationBuilder.DropIndex(
                name: "IX_CandidateDocuments_Session_Exam_Centre",
                table: "CandidateDocuments");

            migrationBuilder.AlterColumn<string>(
                name: "ExamCode",
                table: "CandidateDocuments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CentreCode",
                table: "CandidateDocuments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CandidateNumber",
                table: "CandidateDocuments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CandidateName",
                table: "CandidateDocuments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
