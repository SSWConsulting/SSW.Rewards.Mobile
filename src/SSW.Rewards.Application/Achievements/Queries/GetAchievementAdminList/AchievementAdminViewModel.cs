using SSW.Rewards.Application.Common.Mappings;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementAdminList;

public class AchievementAdminViewModel : IMapFrom<Achievement>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
    public string Code { get; set; }
    public AchievementType Type { get; set; }
    public bool? IsArchived { get; set; } 
}
