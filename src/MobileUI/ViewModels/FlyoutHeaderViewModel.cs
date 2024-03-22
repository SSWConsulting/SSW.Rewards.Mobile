using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class FlyoutHeaderViewModel : ObservableObject
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
    private string _qrCode;

    [ObservableProperty]
    private int _points;

    [ObservableProperty]
    private int _credits;

    [ObservableProperty]
    private int _rank;

    public FlyoutHeaderViewModel(IUserService userService, ILeaderService leaderService)
    {
        _userService = userService;
        _leaderService = leaderService;
        Console.WriteLine($"[FlyoutHeaderViewModel] Email: {Email}");

        _userService.MyName.AsObservable().Subscribe(myName => Name = myName);
        _userService.MyEmail.AsObservable().Subscribe(myEmail => Email = myEmail);
        _userService.MyProfilePic.AsObservable().Subscribe(myProfilePic => ProfilePic = myProfilePic);
        _userService.MyPoints.AsObservable().Subscribe(myPoints => Points = myPoints);
        _userService.MyBalance.AsObservable().Subscribe(myBalance => Credits = myBalance);
        _userService.MyQrCode.AsObservable().Subscribe(myQrCode => QrCode = myQrCode);

        _ = LoadRank();
    }

    private async Task LoadRank()
    {
        var summaries = await _leaderService.GetLeadersAsync(false);
        var myId = _userService.MyUserId.Value;
        Rank = summaries.FirstOrDefault(x => x.UserId == myId)?.Rank ?? 0;
    }
}
