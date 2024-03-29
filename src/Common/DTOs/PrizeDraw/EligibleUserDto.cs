﻿namespace SSW.Rewards.Shared.DTOs.PrizeDraw;

public class EligibleUserDto
{
    public int? UserId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public int TotalPoints { get; set; }

    public int PointsClaimed { get; set; }

    public int PointsToday { get; set; }
    public int PointsThisWeek { get; set; }

    public int PointsThisMonth { get; set; }

    public int PointsThisYear { get; set; }

    public int Balance { get { return TotalPoints - PointsClaimed; } set { _ = value; } }
}
