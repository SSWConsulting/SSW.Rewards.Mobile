namespace SSW.Rewards.Shared.DTOs.Quizzes;

public class QuizDetailsDto
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public Icons Icon { get; set; }
    public string ThumbnailImage { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public DateTime DateCreated { get; set; }
    public IList<QuizQuestionDto> Questions { get; set; } = new List<QuizQuestionDto>();
}
