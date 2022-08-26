namespace SSW.Rewards.Domain.Entities;
public class SubmittedQuizAnswers : BaseEntity
{
    public CompletedQuiz Submission { get; set; }

    public QuizAnswer Answer { get; set; }
}
