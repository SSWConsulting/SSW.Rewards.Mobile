using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class AddQuizCreateAndAttemptRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Quizzes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Passed",
                table: "CompletedQuizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SubmittedQuizAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    AnswerId = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmittedQuizAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmittedQuizAnswers_CompletedQuizzes_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "CompletedQuizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubmittedQuizAnswers_QuizAnswers_AnswerId",
                        column: x => x.AnswerId,
                        principalTable: "QuizAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_CreatedById",
                table: "Quizzes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SubmittedQuizAnswers_AnswerId",
                table: "SubmittedQuizAnswers",
                column: "AnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmittedQuizAnswers_SubmissionId",
                table: "SubmittedQuizAnswers",
                column: "SubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_CreatedById",
                table: "Quizzes",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_CreatedById",
                table: "Quizzes");

            migrationBuilder.DropTable(
                name: "SubmittedQuizAnswers");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_CreatedById",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "Passed",
                table: "CompletedQuizzes");
        }
    }
}
