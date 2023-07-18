using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SSW.Rewards.Mobile.Messages;

public class AchievementTappedMessage : ValueChangedMessage<ProfileAchievement>
{
    public AchievementTappedMessage(ProfileAchievement value) : base(value)
    {
    }
}