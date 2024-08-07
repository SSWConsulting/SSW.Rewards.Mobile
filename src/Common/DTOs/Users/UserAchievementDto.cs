﻿

namespace SSW.Rewards.Shared.DTOs.Users;

public class UserAchievementDto
{
    public int AchievementId { get; set; }

    public required string AchievementName { get; set; }

    public int AchievementValue { get; set; }

    public bool Complete { get; set; }

    public AchievementType AchievementType { get; set; }

    public Icons AchievementIcon { get; set; }

    public bool AchievementIconIsBranded { get; set; }

    public DateTime? AwardedAt { get; set; }
}
