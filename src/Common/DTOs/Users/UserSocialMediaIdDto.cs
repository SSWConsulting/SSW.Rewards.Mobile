namespace SSW.Rewards.Shared.DTOs.Users;

public class UserSocialMediaIdDto
{
    public int SocialMediaPlatformId { get; set; }
    /// <summary>
    /// Profile URL
    /// </summary>
    public required string SocialMediaUserId { get; set; }
}