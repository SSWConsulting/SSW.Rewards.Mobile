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

    public FlyoutHeaderViewModel(IUserService userService)
    {
        WeakReferenceMessenger.Default.Register<UserDetailsUpdatedMessage>(this);
        ProfilePic = userService.MyProfilePic;
        Name = userService.MyName;
        Email = userService.MyEmail;
        IsStaff = userService.IsStaff;
    }

    public void Receive(UserDetailsUpdatedMessage message)
    {
        Console.WriteLine($"[FlyoutHeaderViewModel] Received new user details message: {message.Value.Name}");
        ProfilePic = message.Value.ProfilePic;
        Name = message.Value.Name;
        Email = message.Value.Email;
        IsStaff = message.Value.IsStaff;
    }
}
