using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class FlyoutHeaderViewModel : ObservableObject, IRecipient<UserDetailsUpdatedMessage>
{
    private readonly IUserService _userService;
    private readonly ILeaderService _leaderService;
    
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
    
    [ObservableProperty]
    private int _rank;

    public FlyoutHeaderViewModel(IUserService userService, ILeaderService leaderService)
    {
        WeakReferenceMessenger.Default.Register<UserDetailsUpdatedMessage>(this);
        _userService = userService;
        _leaderService = leaderService;
        ProfilePic = userService.MyProfilePic;
        Name = userService.MyName;
        Email = userService.MyEmail;
        Console.WriteLine($"[FlyoutHeaderViewModel] Email: {Email}");
        IsStaff = userService.IsStaff;
        QrCode = userService.MyQrCode;
        Points = userService.MyPoints;
        Credits = userService.MyBalance;

        _ = LoadRank();
    }
    
    private async Task LoadRank()
    {
        var summaries = await _leaderService.GetLeadersAsync(false);
        var myId = _userService.MyUserId;
        Rank = summaries.FirstOrDefault(x => x.UserId == myId)?.Rank ?? 0;
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
