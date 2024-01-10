namespace SSW.Rewards.Shared.DTOs.Quizzes;
public class QuestionAnswerEditDto
{
    public int QuestionAnswerId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
