﻿using System.Net.Mail;
using System.Text.RegularExpressions;
using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Mobile.ViewModels;

public class LeaderViewModel : BaseViewModel
{
    public int Rank { get; set; }
    public int AllTimeRank { get; set; }
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
    public int Balance { get; set; }
    public bool IsMe { get; set; }
    public bool IsLeader => Rank == 1;
    public string Title { get; set; }
    
    public LeaderViewModel(LeaderboardUserDto dto, bool isMe)
    {
        AllTimeRank = dto.Rank;
        UserId = dto.UserId;
        Name = dto.Name;
        ProfilePic = dto.ProfilePic;
        TotalPoints = dto.TotalPoints;
        PointsThisMonth = dto.PointsThisMonth;
        PointsThisYear = dto.PointsThisYear;
        PointsThisWeek = dto.PointsThisWeek;
        Balance = dto.Balance;
        Email = dto.Email;
        IsMe = isMe;
        Title = Regex.Replace(dto.Title, @"^https?://", string.Empty);
    }

    public int DisplayPoints { get; set; }

    public string DisplayPointsShort
    {
        get => DisplayPoints >= 1000
            ? (DisplayPoints / 1000).ToString("0.#") + "k"
            : DisplayPoints.ToString("#,0");
    }
}
