
namespace SSW.Rewards.Shared.DTOs.PrizeDraw;

public class GetEligibleUsersFilter
{
    public int? AchievementId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool? FilterStaff { get; set; }
    public int? Top { get; set; }
}
