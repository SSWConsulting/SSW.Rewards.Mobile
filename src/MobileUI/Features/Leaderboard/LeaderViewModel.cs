using CommunityToolkit.Mvvm.ComponentModel;
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
    
    // Used for file cache JSON serialization!
    public LeaderViewModel() { }

    public LeaderViewModel(MobileLeaderboardUserDto user)
    {
        IsMe = user.IsMe;
        Rank = user.Rank;
        Name = user.Name;
        UserId = user.UserId;
        ProfilePic = user.ProfilePic;
        _displayPoints = user.Points;

        Title = !string.IsNullOrWhiteSpace(user.Title)
            ? RegexHelpers.WebsiteRegex().Replace(user.Title, string.Empty)
            : string.Empty;
    }

    [ObservableProperty]
    private int _displayPoints;
}
