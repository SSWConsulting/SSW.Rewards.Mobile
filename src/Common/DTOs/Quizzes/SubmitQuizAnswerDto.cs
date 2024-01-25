namespace SSW.Rewards.Shared.DTOs.Quizzes;
public class SubmitQuizAnswerDto
{
    public int SubmissionId { get; set; }
    public int QuestionId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
}
