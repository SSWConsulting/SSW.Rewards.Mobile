namespace SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand;
public class QuizGPTResponseDto
{
    public string QuestionText { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
    public bool Correct { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public int Confidence { get; set; }
}
