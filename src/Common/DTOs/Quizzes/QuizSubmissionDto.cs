namespace SSW.Rewards.Shared.DTOs.Quizzes;

public class QuizSubmissionDto
{
    public int QuizId { get; set; }
    public IList<SubmittedAnswerDto> Answers { get; set; } = new List<SubmittedAnswerDto>();
}
