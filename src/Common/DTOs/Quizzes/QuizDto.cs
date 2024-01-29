namespace SSW.Rewards.Shared.DTOs.Quizzes;

public class QuizDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Passed { get; set; } = false;
    public string ThumbnailImage { get; set; } = string.Empty;
    public string CarouselImage { get; set; } = string.Empty;
    public bool IsCarousel { get; set; } = false;
    
    // TODO: Remove once the ThumbnailImages are working
    public Icons Icon { get; set; }
    public int Points { get; set; }
}
