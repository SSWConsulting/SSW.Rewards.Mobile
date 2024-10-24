using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.PopupPages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class FlyoutHeaderViewModel : ObservableObject
{
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

    public FlyoutHeaderViewModel(IUserService userService, IServiceProvider provider)
    {
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
        Application.Current.Resources.TryGetValue("Background", out var statusBarColor);
        var profilePicVm = ActivatorUtilities.CreateInstance<ProfilePictureViewModel>(_provider);
        var popup = ActivatorUtilities.CreateInstance<ProfilePicturePage>(_provider, profilePicVm, statusBarColor as Color);
        await MopupService.Instance.PushAsync(popup);
    }
}
