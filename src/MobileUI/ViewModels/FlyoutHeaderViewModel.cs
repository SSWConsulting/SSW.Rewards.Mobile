using CommunityToolkit.Mvvm.ComponentModel;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class FlyoutHeaderViewModel : ObservableObject
{
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

    public FlyoutHeaderViewModel(IUserService userService)
    {
        Console.WriteLine($"[FlyoutHeaderViewModel] Email: {Email}");

        userService.MyNameObservable().Subscribe(myName => Name = myName);
        userService.MyEmailObservable().Subscribe(myEmail => Email = myEmail);
        userService.MyProfilePicObservable().Subscribe(myProfilePic => ProfilePic = myProfilePic);
        userService.MyPointsObservable().Subscribe(myPoints => Points = myPoints);
        userService.MyBalanceObservable().Subscribe(myBalance => Credits = myBalance);
        userService.MyQrCodeObservable().Subscribe(myQrCode => QrCode = myQrCode);
        userService.MyAllTimeRankObservable().Subscribe(myRank => Rank = myRank);
    }
}
