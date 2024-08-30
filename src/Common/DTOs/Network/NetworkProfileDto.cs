namespace SSW.Rewards.Shared.DTOs.Users;

public class NetworkProfileDto
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Title { get; set; }
    public string ProfilePicture { get; set; }
    public int TotalPoints { get; set; }
    public int Rank { get; set; }
    public bool IsExternal { get; set; }
    public bool IsStaff { get; set; }
    public int AchievementId { get; set; }
    public bool Scanned { get; set; }
    public bool ScannedMe { get; set; }
}