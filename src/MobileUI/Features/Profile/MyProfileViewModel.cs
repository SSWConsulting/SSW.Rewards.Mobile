using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

public class MyProfileViewModel(
    IUserService userService,
    IDevService devService,
    IServiceProvider provider,
    IFileCacheService fileCacheService,
    IAlertService alertService,
    ILogger<ProfileViewModelBase> logger)
    : ProfileViewModelBase(true, userService, devService, provider, fileCacheService, alertService, logger)
{
    private readonly IUserService _userService = userService;

    // Live only while the page is shown: the singleton UserService would otherwise keep every
    // instance alive and reloading (e.g. a sign-out's user ID 0 would "load" a profile that doesn't exist).
    private CompositeDisposable _subscriptions = new();

    public async Task Initialise()
    {
        _subscriptions.Dispose();
        _subscriptions = new CompositeDisposable(
            _userService.MyUserIdObservable().Subscribe(HandleUserIdChange),
            _userService.MyNameObservable().Subscribe(myName => Name = myName),
            _userService.MyEmailObservable().Subscribe(myEmail => UserEmail = myEmail),
            _userService.MyProfilePicObservable().Subscribe(myProfilePicture => ProfilePic = myProfilePicture),
            _userService.MyPointsObservable().Subscribe(myPoints => Points = myPoints),
            _userService.MyBalanceObservable().Subscribe(myBalance => Balance = myBalance),
            _userService.IsStaffObservable().Subscribe(isStaff => IsStaff = isStaff),
            _userService.MyAllTimeRankObservable().Subscribe(myRank => Rank = myRank));

        await _initialise();
    }

    public override void OnDisappearing()
    {
        _subscriptions.Dispose();
        base.OnDisappearing();
    }

    private async void HandleUserIdChange(int myUserId)
    {
        // 0 = signed out, or the profile hasn't loaded yet: there is no profile to show.
        if (myUserId == 0)
        {
            return;
        }

        UserId = myUserId;
        await LoadProfileSections();
    }
}
