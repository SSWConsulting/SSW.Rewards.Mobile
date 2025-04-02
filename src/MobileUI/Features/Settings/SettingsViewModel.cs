using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Mobile.PopupPages;
using SSW.Rewards.PopupPages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly ISnackbarService _snackbarService;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    private readonly IServiceProvider _provider;
    
    private int _myUserId;
    private string _linkedInUrl;
    private string _gitHubUrl;
    private string _twitterUrl;
    private string _companyUrl;
    
    [ObservableProperty]
    private bool _isStaff;

    public SettingsViewModel(IUserService userService, ISnackbarService snackbarService,
        IFirebaseAnalyticsService firebaseAnalyticsService, IServiceProvider provider)
    {
        _userService = userService;
        _snackbarService = snackbarService;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        _provider = provider;
        Title = "Settings";
        
        _userService.MyUserIdObservable().Subscribe(myUserId => _myUserId = myUserId);
        _userService.IsStaffObservable().Subscribe(isStaff => IsStaff = isStaff);
        _userService.LinkedInProfileObservable().Subscribe(linkedIn => _linkedInUrl = linkedIn);
        _userService.GitHubProfileObservable().Subscribe(gitHub => _gitHubUrl = gitHub);
        _userService.TwitterProfileObservable().Subscribe(twitter => _twitterUrl = twitter);
        _userService.CompanyUrlObservable().Subscribe(company => _companyUrl = company);
    }

    [RelayCommand]
    private async Task IntroClicked()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var page = ActivatorUtilities.CreateInstance<OnBoardingPage>(_provider);
            await MopupService.Instance.PushAsync(page);
        });
    }

    [RelayCommand]
    private static async Task ProfileClicked()
    {
        await AppShell.Current.GoToAsync($"myprofile");
    }

    [RelayCommand]
    private async Task AddLinkedIn()
    {
        await _userService.LoadSocialMedia(_myUserId);
        await EditProfile(Constants.SocialMediaPlatformIds.LinkedIn, _linkedInUrl);
    }
    
    [RelayCommand]
    private async Task AddGitHub()
    {
        await _userService.LoadSocialMedia(_myUserId);
        await EditProfile(Constants.SocialMediaPlatformIds.GitHub, _gitHubUrl);
    }
    
    [RelayCommand]
    private async Task AddTwitter()
    {
        await _userService.LoadSocialMedia(_myUserId);
        await EditProfile(Constants.SocialMediaPlatformIds.Twitter, _twitterUrl);
    }
    
    [RelayCommand]
    private async Task AddCompany()
    {
        await _userService.LoadSocialMedia(_myUserId);
        await EditProfile(Constants.SocialMediaPlatformIds.Company, _companyUrl);
    }
    
    private async Task EditProfile(int socialMediaPlatformId, string currentUrl) {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var page = ActivatorUtilities.CreateInstance<AddSocialMediaPage>(_provider, socialMediaPlatformId, currentUrl);
            await MopupService.Instance.PushAsync(page);
        });
    }

    [RelayCommand]
    private async Task DeleteClicked()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var popup = ActivatorUtilities.CreateInstance<DeleteProfilePage>(_provider);
            await MopupService.Instance.PushAsync(popup);
        });
    }

    [RelayCommand]
    private async Task AboutClicked()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var popup = new AboutSswPage(_firebaseAnalyticsService);
            await MopupService.Instance.PushAsync(popup);
        });
    }
}