namespace Shared.DTOs.Users;
public class ProfilePicResponseDto
{
    public required string PicUrl { get; set; }

    public bool AchievementAwarded { get; set; } = false;
}
