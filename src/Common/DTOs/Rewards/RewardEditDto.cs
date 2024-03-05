namespace SSW.Rewards.Shared.DTOs.Rewards;

public class RewardEditDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsHidden { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Cost { get; set; }
    public string ImageUri { get; set; }
    public string CarouselImageUri { get; set; }
    public bool IsCarousel { get; set; }
    public RewardType RewardType { get; set; }
    public string ImageBytesInBase64 { get; set; }
    public string ImageFileName { get; set; }    
    public string CarouselImageBytesInBase64 { get; set; }
    public string CarouselImageFileName { get; set; }
    public bool? IsOnboardingReward { get; set; }
}
