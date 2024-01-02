using CommunityToolkit.Mvvm.Messaging.Messages;
using SSW.Rewards.Mobile.Models;

namespace SSW.Rewards.Mobile.Messages;

public class SocialUsernameAddedMessage : ValueChangedMessage<SocialAchievement>
{
    public SocialUsernameAddedMessage(SocialAchievement value) : base(value)
    {
    }
}