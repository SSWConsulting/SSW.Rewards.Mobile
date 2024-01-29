namespace SSW.Rewards.Domain.Entities;
public class Quiz : BaseEntity
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? CarouselImage { get; set; } = string.Empty;    
    public string? ThumbnailImage { get; set; } = string.Empty;
    public bool IsCarousel { get; set; } = false;
    public Icons Icon { get; set; }
    public DateTime? LastUpdatedUtc { get; set; }
    public bool IsArchived { get; set; }
    public int AchievementId { get; set; }
    public virtual Achievement Achievement { get; set; }
    public ICollection<QuizQuestion> Questions { get; set; } = new HashSet<QuizQuestion>();
    public ICollection<CompletedQuiz> CompletedQuizzes { get; set; } = new HashSet<CompletedQuiz>();
    public User CreatedBy { get; set; } = null!;
    public int? CreatedById { get; set; }
}
