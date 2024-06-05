namespace SSW.Rewards.Shared.DTOs.Rewards;

public class RewardDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Code { get; set; }
    public int Cost { get; set; }
    public string? ImageUri { get; set; }
    public string? CarouselImageUri { get; set; }
    public bool IsCarousel { get; set; }
    public bool IsHidden { get; set; }
    
    public RewardType RewardType { get; set; }
}
