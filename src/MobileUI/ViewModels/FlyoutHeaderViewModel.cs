using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class FlyoutHeaderViewModel : ObservableObject
{
    private int _myUserId;

    private readonly ILeaderService _leaderService;

    [ObservableProperty]
    private string _profilePic;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _email;

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
        _leaderService = leaderService;
        Console.WriteLine($"[FlyoutHeaderViewModel] Email: {Email}");

        userService.MyUserIdObservable().Subscribe(myUserId => _myUserId = myUserId);
        userService.MyNameObservable().Subscribe(myName => Name = myName);
        userService.MyEmailObservable().Subscribe(myEmail => Email = myEmail);
        userService.MyProfilePicObservable().Subscribe(myProfilePic => ProfilePic = myProfilePic);
        userService.MyPointsObservable().Subscribe(myPoints => Points = myPoints);
        userService.MyBalanceObservable().Subscribe(myBalance => Credits = myBalance);
        userService.MyQrCodeObservable().Subscribe(myQrCode => QrCode = myQrCode);

        _ = LoadRank();
    }

    private async Task LoadRank()
    {
        var summaries = await _leaderService.GetLeadersAsync(false);
        Rank = summaries.FirstOrDefault(x => x.UserId == _myUserId)?.Rank ?? 0;
    }
}
