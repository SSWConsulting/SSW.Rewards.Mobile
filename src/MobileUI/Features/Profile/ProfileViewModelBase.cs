using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.PopupPages;
using SSW.Rewards.PopupPages;
using SSW.Rewards.Shared.DTOs.Staff;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class ProfileViewModelBase : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly IDevService _devService;
    private readonly IPermissionsService _permissionsService;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    private readonly IServiceProvider _provider;

    [ObservableProperty]
    private string _profilePic;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private int _points;

    [ObservableProperty]
    private int _balance;

    [ObservableProperty]
    private int _rank;

    [ObservableProperty]
    private string _userEmail;

    [ObservableProperty]
    private bool _isStaff;
    
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _linkedInUrl;
    
    [ObservableProperty]
    private string _gitHubUrl;
    
    [ObservableProperty]
    private string _twitterUrl;
    
    [ObservableProperty]
    private string _companyUrl;

    public bool ShowBalance { get; set; } = true;

    protected int UserId { get; set; }

    public bool ShowCloseButton { get; set; } = true;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isMe;

    public ObservableCollection<Activity> RecentActivity { get; } = [];
    public ObservableCollection<Activity> LastSeen { get; } = [];
    public ObservableCollection<StaffSkillDto> Skills { get; set; } = [];
    private readonly SemaphoreSlim _loadingProfileSectionsSemaphore = new(1,1);

    public ProfileViewModelBase(
        bool isMe,
        IUserService userService,
        IDevService devService,
        IPermissionsService permissionsService,
        IFirebaseAnalyticsService firebaseAnalyticsService,
        IServiceProvider provider)
    {
        IsMe = isMe;
        _userService = userService;
        _devService = devService;
        _permissionsService = permissionsService;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        _provider = provider;
        
        userService.LinkedInProfileObservable().Subscribe(linkedIn => LinkedInUrl = linkedIn);
        userService.GitHubProfileObservable().Subscribe(gitHub => GitHubUrl = gitHub);
        userService.TwitterProfileObservable().Subscribe(twitter => TwitterUrl = twitter);
        userService.CompanyUrlObservable().Subscribe(company => CompanyUrl = company);
    }

    protected async Task _initialise()
    {
        await LoadProfileSections();
    }

    protected async Task LoadProfileSections()
    {
        if (!_loadingProfileSectionsSemaphore.Wait(0))
            return;

        try
        {
            IsLoading = true;
            var profileTask = _userService.GetUserAsync(UserId);
            var socialMediaTask = LoadSocialMedia();
        
            await Task.WhenAll(profileTask, socialMediaTask);

            var profile = profileTask.Result;

            ProfilePic = profile.ProfilePic ?? "v2sophie";
            Name = profile.FullName;
            Rank = profile.Rank;
            Points = profile.Points;
            Balance = profile.Balance;
            IsStaff = profile.IsStaff;
            UserEmail = profile.Email;
            Title = GetTitle();
        
            await UpdateSkillsSectionIfRequired();
            UpdateLastSeenSection(profile.Achievements);
            UpdateRecentActivitySection(profile.Achievements, profile.Rewards);
        }
        catch (Exception ex)
        {
            await ClosePage();
            await App.Current.MainPage.DisplayAlert("Oops...", "There was an error loading this profile", "OK");
        }

        _loadingProfileSectionsSemaphore.Release();
        IsLoading = false;
    }

    private async Task LoadSocialMedia()
    {
        await _userService.LoadSocialMedia(UserId);
    }

    private string GetTitle()
    {
        if (IsStaff)
        {
            return "SSW";
        }

        return !string.IsNullOrEmpty(CompanyUrl)
            ? Regex.Replace(CompanyUrl, @"^https?://", string.Empty)
            : "Community";
    }

    [RelayCommand]
    private async Task ChangeProfilePicture()
    {
        if (IsLoading || !IsMe)
            return;

        Application.Current.Resources.TryGetValue("Background", out var statusBarColor);
        var popup = new ProfilePicturePage(new ProfilePictureViewModel(_userService, _permissionsService), _firebaseAnalyticsService, statusBarColor as Color);
        await MopupService.Instance.PushAsync(popup);
    }

    [RelayCommand]
    private async Task OpenLinkedInProfile()
    {
        await OpenProfile(LinkedInUrl, Constants.SocialMediaPlatformIds.LinkedIn);
    }
    
    [RelayCommand]
    private async Task OpenGitHubProfile()
    {
        await OpenProfile(GitHubUrl, Constants.SocialMediaPlatformIds.GitHub);
    }
    
    [RelayCommand]
    private async Task OpenTwitterProfile()
    {
        await OpenProfile(TwitterUrl, Constants.SocialMediaPlatformIds.Twitter);
    }
    
    [RelayCommand]
    private async Task OpenCompanyUrl()
    {
        await OpenProfile(CompanyUrl, Constants.SocialMediaPlatformIds.Company);
    }

    private async Task OpenProfile(string userProfile, int socialMediaPlatformId) {
        if (string.IsNullOrWhiteSpace(userProfile))
        {
            if (!IsMe)
            {
                return;
            }

            Application.Current.Resources.TryGetValue("Background", out var statusBarColor);
            var page = ActivatorUtilities.CreateInstance<AddSocialMediaPage>(_provider, socialMediaPlatformId, string.Empty, statusBarColor as Color);
            await MopupService.Instance.PushAsync(page);
            return;
        }

        if (Uri.TryCreate(userProfile, UriKind.Absolute, out Uri uri))
        {
            try
            {
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.External);
            }
            catch (Exception)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "There was an error trying to launch the default browser.", "OK");
            }
        }
    }

    public string GetMessage(UserAchievementDto achievement, bool isActivity = false)
    {
        string prefix = IsMe ? "You have" : $"{Name} has";

        if (!achievement.Complete)
        {
            prefix += " not";
        }

        string activity = achievement.AchievementName;
        string action = string.Empty;
        string scored = $"just scored {achievement.AchievementValue}pts for";

        switch (achievement.AchievementType)
        {
            case AchievementType.Attended:
                action = "checked into";
                break;

            case AchievementType.Completed:
                action = $"{scored} completing";
                break;

            case AchievementType.Linked:
                action = $"{scored} linking";
                activity = activity.Split(' ').Last();
                break;

            case AchievementType.Scanned:
                action = $"{scored} scanning";
                break;
        }

        if (isActivity)
        {
            action = char.ToUpper(action[0]) + action.Substring(1);
            return $"{action} {activity}";
        }
        else
        {
            return $"{prefix} {action} {activity}";
        }
    }

    private void UpdateLastSeenSection(IEnumerable<UserAchievementDto> achievementList)
    {
        LastSeen.Clear(); // it could contain data from another user profile
        var recentLastSeen = achievementList.Where(a => a.AchievementType == AchievementType.Attended).OrderByDescending(a => a.AwardedAt).Take(5);
        foreach (var achievement in recentLastSeen)
        {
            LastSeen.Add(new Activity
            {
                ActivityName = GetMessage(achievement, true),
                OccurredAt = achievement.AwardedAt,
                Type = achievement.AchievementType.ToActivityType(),
                TimeElapsed = DateTimeHelpers.GetTimeElapsed(achievement.AwardedAt.Value)
            });
        }
    }

    private void UpdateRecentActivitySection(IEnumerable<UserAchievementDto> achievements, IEnumerable<UserRewardDto> rewards)
    {
        const int takeSize = 5;
        List<Activity> activities = [];
        
        activities.AddRange(FilterRecentAchievements(achievements, takeSize));
        activities.AddRange(FilterRecentRewards(rewards, takeSize));

        RecentActivity.Clear(); // it could contain data from another user profile
        var recentActivity = activities.OrderByDescending(a => a.OccurredAt).Take(takeSize);
        foreach (var activity in recentActivity)
        {
            RecentActivity.Add(activity);
        }
    }

    private IEnumerable<Activity> FilterRecentAchievements(IEnumerable<UserAchievementDto> achievementList, int takeSize)
    {
        List<Activity> result = [];
        var recentAchievements = achievementList
            .Where(a => a.AchievementType != AchievementType.Attended)
            .OrderByDescending(a => a.AwardedAt)
            .Take(takeSize);
        
        foreach (var achievement in recentAchievements)
        {
            result.Add(new Activity
            {
                ActivityName = GetMessage(achievement, true),
                OccurredAt = achievement.AwardedAt,
                Type = achievement.AchievementType.ToActivityType(),
                TimeElapsed = DateTimeHelpers.GetTimeElapsed(achievement.AwardedAt.Value)
            });
        }

        return result;
    }
    
    private static IEnumerable<Activity> FilterRecentRewards(IEnumerable<UserRewardDto> rewardList, int takeSize)
    {
        List<Activity> result = [];
        var recentRewards = rewardList
            .Where(r => r.Awarded)
            .OrderByDescending(r => r.AwardedAt)
            .Take(takeSize);

        foreach (var reward in recentRewards)
        {
            result.Add(new Activity
            {
                ActivityName = $"Claimed {reward.RewardName}",
                OccurredAt = reward.AwardedAt,
                Type = ActivityType.Claimed,
                TimeElapsed = DateTimeHelpers.GetTimeElapsed((DateTime)reward.AwardedAt)
            });
        }

        return result;
    }

    private async Task UpdateSkillsSectionIfRequired()
    {
        if (IsStaff)
        {
            DevProfile devProfile = await _devService.GetProfileAsync(UserEmail);
            if (devProfile != null)
            {
                Skills.Clear(); // it could contain data from another user profile
                foreach (var skill in devProfile.Skills.OrderByDescending(s => s.Level).Take(3))
                {
                    Skills.Add(skill);
                }
            }
        }
    }

    public void OnDisappearing()
    {
        IsBusy = false;
        IsLoading = false;
        // Clearing LinkedIn profile so that the previous value doesn't display during page loading
        _userService.ClearSocialMedia();
    }

    [RelayCommand]
    private async Task ClosePage()
    {
        await Navigation.PopModalAsync();
    }
}