using System.Text.RegularExpressions;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Shared.DTOs.Leaderboard;

public class LeaderboardUserDto
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? ProfilePic { get; set; }
    public int TotalPoints { get; set; }
    public int PointsClaimed { get; set; }
    public int PointsToday { get; set; }
    public int PointsThisWeek { get; set; }
    public int PointsThisMonth { get; set; }
    public int PointsThisYear { get; set; }
    public int Balance => TotalPoints - PointsClaimed;
    public string Title { get; set; }

    public LeaderboardUserDto() {}

    public LeaderboardUserDto(User user)
    {
        Rank = 0; // Ignored
        UserId = user.Id;
        Name = user.FullName;
        Email = user.Email;
        ProfilePic = user.Avatar;
        TotalPoints = user.UserAchievements.Sum(ua => ua.Achievement.Value);
        PointsClaimed = user.UserRewards.Sum(ur => ur.Reward.Cost);
        PointsToday = user.UserAchievements
            .Where(ua => 
                ua.AwardedAt.Year == DateTime.UtcNow.Year && 
                ua.AwardedAt.Month == DateTime.UtcNow.Month && 
                ua.AwardedAt.Day == DateTime.UtcNow.Day)
            .Sum(ua => ua.Achievement.Value);
        PointsThisWeek = user.UserAchievements
            .Where(ua => DateTime.UtcNow.AddDays(-7) <= ua.AwardedAt && ua.AwardedAt <= DateTime.UtcNow)
            .Sum(ua => ua.Achievement.Value);
        PointsThisMonth = user.UserAchievements
            .Where(ua => ua.AwardedAt.Year == DateTime.UtcNow.Year && ua.AwardedAt.Month == DateTime.UtcNow.Month)
            .Sum(ua => ua.Achievement.Value);
        PointsThisYear = user.UserAchievements
            .Where(ua => ua.AwardedAt.Year == DateTime.UtcNow.Year)
            .Sum(ua => ua.Achievement.Value);

        var company = user.SocialMediaIds.FirstOrDefault(s => s.SocialMediaPlatform.Name == "Company")
            ?.SocialMediaUserId;

        if (!string.IsNullOrEmpty(company))
        {
            Title = Regex.Replace(company, @"^https?://", string.Empty);
        }
        else
        {
            Title = user.Email?.EndsWith("ssw.com.au") == true ? "SSW" : "Community";
        }
    }
}
