using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SSW.Rewards.Mobile.Messages;

public class UserDetailsUpdatedMessage : ValueChangedMessage<UserContext>
{
    public UserDetailsUpdatedMessage(UserContext value) : base(value)
    {
    }
}
