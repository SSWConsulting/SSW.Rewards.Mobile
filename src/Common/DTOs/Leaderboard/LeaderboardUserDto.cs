using System.Text.Json.Serialization;

namespace SSW.Rewards.Shared.DTOs.Leaderboard;

public class LeaderboardUserDto
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? ProfilePic { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int TotalPoints { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int PointsClaimed { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int PointsToday { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int PointsThisWeek { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int PointsThisMonth { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int PointsThisYear { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Balance => TotalPoints - PointsClaimed;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Title { get; set; }
}
