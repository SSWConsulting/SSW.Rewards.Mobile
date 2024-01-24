namespace SSW.Rewards.Shared.DTOs.Quizzes;
public class BeginQuizDto
{
    public int SubmissionId { get; set; }
    public List<QuizQuestionDto> Questions { get; set; } = new();
}
