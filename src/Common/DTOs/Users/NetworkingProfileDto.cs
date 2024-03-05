namespace SSW.Rewards.Shared.DTOs.Users;

public class NetworkingProfileDto
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public string ProfilePicture { get; set; }
    public bool IsExternal { get; set; }
    public int AchievementId { get; set; }
    public bool Scanned { get; set; }
}