using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    public partial class AddQuizAttemptRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletedQuizId",
                table: "QuizAnswers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Passed",
                table: "CompletedQuizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_CompletedQuizId",
                table: "QuizAnswers",
                column: "CompletedQuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAnswers_CompletedQuizzes_CompletedQuizId",
                table: "QuizAnswers",
                column: "CompletedQuizId",
                principalTable: "CompletedQuizzes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAnswers_CompletedQuizzes_CompletedQuizId",
                table: "QuizAnswers");

            migrationBuilder.DropIndex(
                name: "IX_QuizAnswers_CompletedQuizId",
                table: "QuizAnswers");

            migrationBuilder.DropColumn(
                name: "CompletedQuizId",
                table: "QuizAnswers");

            migrationBuilder.DropColumn(
                name: "Passed",
                table: "CompletedQuizzes");
        }
    }
}
