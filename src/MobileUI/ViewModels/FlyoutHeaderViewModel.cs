using System.Reactive.Linq;
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

        ProfilePic = _userService.MyProfilePic;
        Name = _userService.MyName;
        Console.WriteLine($"[FlyoutHeaderViewModel] Email: {Email}");
        IsStaff = _userService.IsStaff;
        AppShell.ProfilePic = ProfilePic;

        _userService.MyEmail.AsObservable().Subscribe(myEmail => Email = myEmail);
        _userService.MyPoints.AsObservable().Subscribe(myPoints => Points = myPoints);
        _userService.MyBalance.AsObservable().Subscribe(myBalance => Credits = myBalance);

        UpdateUserValues();

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
        IsStaff = message.Value.IsStaff;

        UpdateUserValues();
    }

    private void UpdateUserValues()
    {
        QrCode = _userService.MyQrCode;
    }
}
