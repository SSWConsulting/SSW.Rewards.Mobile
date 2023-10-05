using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public class ProfileCarouselViewModel
{
    public CarouselType Type { get; set; }

    public List<ProfileAchievement> Achievements { get; set; } = new List<ProfileAchievement>();

    public List<Activity> RecentActivity { get; set; } = new List<Activity>();

    public List<Notification> Notifications { get; set; } = new List<Notification>();

    public bool IsMe { get; set; }

    public string ProfileName { get; set; }

    public string EmptyHeader
    {
        get
        {
            return IsMe ? "You have no recent activity." : $"{ProfileName} has no recent activity.";
        }
    }
}

public enum CarouselType
{
    Achievements,
    RecentActivity,
    Notifications
}

public class ProfileAchievement : Achievement
{
    public ICommand AchievementTappedCommand { get; set; }

    public static string AchievementTappedMessage = "AchivementTapped";

    public bool IsMe { get; set; }

    public ProfileAchievement()
    {
        //AchievementTappedCommand = new Command(() =>
        //    WeakReferenceMessenger.Default.Send(new AchievementTappedMessage(this)));

        AchievementTappedCommand = new Command(() => WeakReferenceMessenger.Default.Send(new AchievementTappedMessage(this)));
    }
}

public static class AchievementHelpers
{
    public static ProfileAchievement ToProfileAchievement(this Achievement achievement, bool isMe)
    {
        return new ProfileAchievement
        {
            AwardedAt = achievement.AwardedAt,
            Complete = achievement.Complete,
            Name = achievement.Name,
            Type = achievement.Type,
            Value = achievement.Value,
            AchievementIcon = achievement.AchievementIcon,
            IconIsBranded = achievement.IconIsBranded,
            Id = achievement.Id,
            IsMe = isMe
        };
    }
}
