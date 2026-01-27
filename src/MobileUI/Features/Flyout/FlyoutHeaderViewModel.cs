using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.PopupPages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class FlyoutHeaderViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly IPermissionsService _permissionsService;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    private readonly IServiceProvider _provider;

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

    public FlyoutHeaderViewModel(IUserService userService, IPermissionsService permissionsService, IFirebaseAnalyticsService firebaseAnalyticsService, IServiceProvider provider)
    {
        _userService = userService;
        _permissionsService = permissionsService;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        _provider = provider;
        Console.WriteLine($"[FlyoutHeaderViewModel] Email: {Email}");

        userService.MyNameObservable().Subscribe(myName => Name = myName);
        userService.MyEmailObservable().Subscribe(myEmail => Email = myEmail);
        userService.MyProfilePicObservable().Subscribe(myProfilePic => ProfilePic = myProfilePic);
        userService.MyPointsObservable().Subscribe(myPoints => Points = myPoints);
        userService.MyBalanceObservable().Subscribe(myBalance => Credits = myBalance);
        userService.MyQrCodeObservable().Subscribe(myQrCode => QrCode = myQrCode);
        userService.MyAllTimeRankObservable().Subscribe(myRank => Rank = myRank);
    }

    [RelayCommand]
    private async Task ChangeProfilePicture()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var vm = ActivatorUtilities.CreateInstance<ProfilePictureViewModel>(_provider);
            var popup = ActivatorUtilities.CreateInstance<ProfilePicturePage>(_provider, vm);
            await MopupService.Instance.PushAsync(popup);
        });
    }
}
