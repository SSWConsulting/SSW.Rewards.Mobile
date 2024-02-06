namespace SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand;

public class QuizGPTRequestDto
{
    public string QuestionText { get; set; } = "";
    public string AnswerText { get; set; } = "";
    public string? BenchmarkAnswer { get; set; }
}
