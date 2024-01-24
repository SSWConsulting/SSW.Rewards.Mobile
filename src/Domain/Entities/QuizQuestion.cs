namespace SSW.Rewards.Domain.Entities;
public class QuizQuestion : BaseEntity
{
    public int QuizId { get; set; }
    public virtual Quiz Quiz { get; set; } = null!;
    public string? Text { get; set; } = string.Empty;
    public virtual ICollection<QuizAnswer> Answers { get; set; } = new HashSet<QuizAnswer>();
    public virtual ICollection<SubmittedQuizAnswer> UserAnswers { get; set; } = new HashSet<SubmittedQuizAnswer>();
}
