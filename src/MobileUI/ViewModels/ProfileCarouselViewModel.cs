using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public class ProfileCarouselViewModel
{
    public List<Activity> RecentActivity { get; set; } = [];

    public bool IsMe { get; set; }

    public string ProfileName { get; set; }

    public string EmptyHeader
    {
        get => IsMe ? "You have no recent activity." : $"{ProfileName} has no recent activity.";
    }
}
