using CommunityToolkit.Mvvm.ComponentModel;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.Leaderboard;
using SSW.Rewards.Shared.Utils;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class LeaderViewModel : BaseViewModel
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? ProfilePic { get; set; }
    public bool IsMe { get; set; }
    public bool IsLeader => Rank == 1;
    
    public LeaderViewModel(LeaderboardUserDto dto, bool isMe)
    {
        UserId = dto.UserId;
        Name = dto.Name;
        ProfilePic = dto.ProfilePic;
        IsMe = isMe;
        Title = RegexHelpers.TitleRegex().Replace(dto.Title, string.Empty);
    }

    public LeaderViewModel(LeaderboardUserDto dto, bool isMe, int rank, LeaderboardFilter period)
        : this(dto, isMe)
    {
        Rank = rank;
        _displayPoints = CalculateDisplayPoints(dto, period);
    }

    private static int CalculateDisplayPoints(LeaderboardUserDto user, LeaderboardFilter period)
        => period switch
        {
            LeaderboardFilter.ThisMonth => user.PointsThisMonth,
            LeaderboardFilter.ThisYear => user.PointsThisYear,
            LeaderboardFilter.Forever => user.TotalPoints,
            _ => user.PointsThisWeek
        };

    [ObservableProperty]
    private int _displayPoints;
}
