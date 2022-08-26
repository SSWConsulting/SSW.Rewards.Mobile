namespace SSW.Rewards.Domain.Entities;
public class SubmittedQuizAnswer : BaseEntity
{
    public int SubmissionId { get; set; }
    public CompletedQuiz Submission { get; set; }

    public int AnswerId { get; set; }
    public QuizAnswer Answer { get; set; }
}
