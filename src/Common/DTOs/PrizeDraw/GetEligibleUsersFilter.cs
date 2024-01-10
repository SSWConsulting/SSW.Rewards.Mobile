
namespace SSW.Rewards.Shared.DTOs.PrizeDraw;

public class GetEligibleUsersFilter
{
    public int? AchievementId { get; set; }
    public LeaderboardFilter? Filter { get; set; }
    public bool? FilterStaff { get; set; }
}
