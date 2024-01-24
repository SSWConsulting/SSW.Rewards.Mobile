using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSW.Rewards.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGPTQuizFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnswerText",
                table: "SubmittedAnswers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Correct",
                table: "SubmittedAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GPTConfidence",
                table: "SubmittedAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GPTExplanation",
                table: "SubmittedAnswers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "QuizQuestionId",
                table: "SubmittedAnswers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmittedAnswers_QuizQuestionId",
                table: "SubmittedAnswers",
                column: "QuizQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubmittedAnswers_QuizQuestions_QuizQuestionId",
                table: "SubmittedAnswers",
                column: "QuizQuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubmittedAnswers_QuizQuestions_QuizQuestionId",
                table: "SubmittedAnswers");

            migrationBuilder.DropIndex(
                name: "IX_SubmittedAnswers_QuizQuestionId",
                table: "SubmittedAnswers");

            migrationBuilder.DropColumn(
                name: "AnswerText",
                table: "SubmittedAnswers");

            migrationBuilder.DropColumn(
                name: "Correct",
                table: "SubmittedAnswers");

            migrationBuilder.DropColumn(
                name: "GPTConfidence",
                table: "SubmittedAnswers");

            migrationBuilder.DropColumn(
                name: "GPTExplanation",
                table: "SubmittedAnswers");

            migrationBuilder.DropColumn(
                name: "QuizQuestionId",
                table: "SubmittedAnswers");
        }
    }
}
