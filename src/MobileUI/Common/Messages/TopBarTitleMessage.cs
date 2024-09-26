using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SSW.Rewards.Mobile.Messages;

public class TopBarTitleMessage(string value) : ValueChangedMessage<string>(value);