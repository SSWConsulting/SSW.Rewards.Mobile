using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class FlyoutHeaderViewModel : ObservableObject, IRecipient<UserDetailsUpdatedMessage>
{
    [ObservableProperty]
    private string profilePic;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private bool isStaff;

    public FlyoutHeaderViewModel()
    {
        WeakReferenceMessenger.Default.Register<UserDetailsUpdatedMessage>(this);
    }

    public void Receive(UserDetailsUpdatedMessage message)
    {
        ProfilePic = message.ProfilePic;
        Name = message.Name;
        Email = message.Email;
        IsStaff = message.IsStaff;
    }
}
