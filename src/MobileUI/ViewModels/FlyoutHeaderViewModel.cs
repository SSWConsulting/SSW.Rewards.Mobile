using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class FlyoutHeaderViewModel : ObservableObject, IRecipient<UserDetailsUpdatedMessage>
{
    [ObservableProperty]
    private string _profilePic;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _email;

    [ObservableProperty]
    private bool _isStaff;
    
    [ObservableProperty]
    private string _qrCode;
    
    [ObservableProperty]
    private int _points;
    
    [ObservableProperty]
    private int _credits;

    public FlyoutHeaderViewModel(IUserService userService)
    {
        WeakReferenceMessenger.Default.Register<UserDetailsUpdatedMessage>(this);
        ProfilePic = userService.MyProfilePic;
        Name = userService.MyName;
        Email = userService.MyEmail;
        Console.WriteLine($"[FlyoutHeaderViewModel] Email: {Email}");
        IsStaff = userService.IsStaff;
        QrCode = userService.MyQrCode;
        Points = userService.MyPoints;
        Credits = userService.MyBalance;
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
