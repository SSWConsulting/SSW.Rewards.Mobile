using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SSW.Rewards.Mobile.Messages;
public class TopBarAvatarMessage : ValueChangedMessage<AvatarOptions>
{
    public TopBarAvatarMessage(AvatarOptions value) : base(value)
    {
    }
}

public enum AvatarOptions
{
    Back,
    Original
}