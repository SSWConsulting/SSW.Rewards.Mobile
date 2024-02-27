namespace SSW.Rewards.Shared.DTOs.Rewards;

public class RewardDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Cost { get; set; }
    public string? ImageUri { get; set; }
    public string? CarouselImageUri { get; set; }
    public bool IsCarousel { get; set; }
    
    public RewardType RewardType { get; set; }
}
