namespace SSW.Rewards.Domain.Entities;
public class CompletedQuiz : BaseEntity
{
    public int UserId { get; set; }
    public virtual User User { get; set; }
    public int QuizId { get; set; }
    public virtual Quiz Quiz { get; set; }
    public bool Passed { get; set; }
    public List<SubmittedQuizAnswer> Answers { get; set; } = new();
}
