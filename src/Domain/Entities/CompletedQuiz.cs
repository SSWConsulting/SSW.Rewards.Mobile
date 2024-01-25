namespace SSW.Rewards.Domain.Entities;

public class CompletedQuiz : BaseEntity
{
    public int UserId { get; set; }
    public virtual User User { get; set; }
    public int QuizId { get; set; }
    public virtual Quiz Quiz { get; set; }
    public bool Passed { get; set; }
    public List<SubmittedQuizAnswer> Answers { get; set; } = new();

    // TODO: Add a start date/time so we can add a time limit to completing the quiz
    // as this record should be created when they first begin the quiz.
}
