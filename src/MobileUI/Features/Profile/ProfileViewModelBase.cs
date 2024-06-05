using System.Collections.ObjectModel;
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
    private readonly IRewardService _rewardsService;
    private readonly IUserService _userService;
    private readonly IDevService _devService;
    private readonly IPermissionsService _permissionsService;
    private readonly ISnackbarService _snackbarService;

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
    private string? _linkedInUrl;

    public bool ShowBalance { get; set; } = true;

    protected int UserId { get; set; }

    public bool ShowCloseButton { get; set; } = true;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isMe;

    [ObservableProperty]
    private Color _linkedInColor;

    [ObservableProperty]
    private Color _gitHubColor = Colors.DimGrey;

    [ObservableProperty]
    private Color _twitterColor = Colors.DimGrey;

    public ObservableCollection<Activity> RecentActivity { get; } = [];
    public ObservableCollection<Activity> LastSeen { get; } = [];
    public ObservableCollection<StaffSkillDto> Skills { get; set; } = [];
    private readonly SemaphoreSlim _loadingProfileSectionsSemaphore = new(1,1);

    public ProfileViewModelBase(
        bool isMe,
        IRewardService rewardsService,
        IUserService userService,
        IDevService devService,
        IPermissionsService permissionsService,
        ISnackbarService snackbarService)
    {
        IsMe = isMe;
        _rewardsService = rewardsService;
        _userService = userService;
        _devService = devService;
        _permissionsService = permissionsService;
        _snackbarService = snackbarService;
        userService.LinkedInProfileObservable().Subscribe(myLinkedIn =>
        {
            LinkedInUrl = myLinkedIn;
            App.Current.Resources.TryGetValue("SSWRed", out object color);
            var sswRed = (Color)color!;
            LinkedInColor = !string.IsNullOrWhiteSpace(myLinkedIn)
                ? sswRed
                : IsMe
                    ? Colors.White
                    : Colors.DimGrey;
        });
    }

    protected async Task _initialise()
    {
        IsLoading = true;
        await LoadProfileSections();
        IsLoading = false;
    }

    protected async Task LoadProfileSections()
    {
        if (!_loadingProfileSectionsSemaphore.Wait(0))
            return;

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
        
        UpdateLastSeenSection(profile.Achievements);
        UpdateRecentActivitySection(profile.Achievements, profile.Rewards);
        await UpdateSkillsSectionIfRequired();

        _loadingProfileSectionsSemaphore.Release();
    }

    private async Task LoadSocialMedia()
    {
        await _userService.LoadSocialMedia(UserId, Constants.SocialMediaPlatformIds.LinkedIn);
    }

    [RelayCommand]
    private async Task ChangeProfilePicture()
    {
        if (IsLoading || !IsMe)
            return;

        Application.Current.Resources.TryGetValue("Background", out var statusBarColor);
        var popup = new ProfilePicturePage(new ProfilePictureViewModel(_userService, _permissionsService), statusBarColor as Color);
        await MopupService.Instance.PushAsync(popup);
    }

    [RelayCommand]
    private async Task OpenLinkedInProfile()
    {
        if (string.IsNullOrWhiteSpace(LinkedInUrl))
        {
            if (!IsMe)
            {
                return;
            }

            Application.Current.Resources.TryGetValue("Background", out var statusBarColor);
            var page = new AddLinkedInPage(_userService, _snackbarService, statusBarColor as Color);
            await MopupService.Instance.PushAsync(page);
            return;
        }

        if (Uri.TryCreate(LinkedInUrl, UriKind.Absolute, out Uri uri))
        {
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
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
                action = $"{scored} following";
                activity = activity.Replace("follow", "");
                activity = activity.Replace("Follow", "");
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
                TimeElapsed = GetTimeElapsed(achievement.AwardedAt.Value)
            });
        }
    }

    private void UpdateRecentActivitySection(IEnumerable<UserAchievementDto> achievementList, IEnumerable<UserRewardDto> rewardList)
    {
        var activityList = new List<Activity>();
        var recentAchievements = achievementList.Where(a => a.AchievementType != AchievementType.Attended).OrderByDescending(a => a.AwardedAt).Take(5);
        foreach (var achievement in recentAchievements)
        {
            activityList.Add(new Activity
            {
                ActivityName = GetMessage(achievement, true),
                OccurredAt = achievement.AwardedAt,
                Type = achievement.AchievementType.ToActivityType(),
                TimeElapsed = GetTimeElapsed(achievement.AwardedAt.Value)
            });
        }

        var recentRewards = rewardList
            .Where(r => r.Awarded)
            .OrderByDescending(r => r.AwardedAt).Take(5);

        foreach (var reward in recentRewards)
        {
            activityList.Add(new Activity
            {
                ActivityName = $"Claimed {reward.RewardName}",
                OccurredAt = reward.AwardedAt,
                Type = ActivityType.Claimed,
                TimeElapsed = GetTimeElapsed((DateTime)reward.AwardedAt)
            });
        }

        RecentActivity.Clear(); // it could contain data from another user profile
        foreach (var activity in activityList.OrderByDescending(a => a.OccurredAt).Take(5))
        {
            RecentActivity.Add(activity);
        }
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

    private static string GetTimeElapsed(DateTime occurredAt)
    {
        return (DateTime.UtcNow - occurredAt) switch
        {
            { TotalDays: < 1 } ts => $"{ts.Hours}h",
            { TotalDays: < 31 } ts => $"{ts.Days}d",
            _ => occurredAt.ToLocalTime().ToString("dd MMMM yyyy"),
        };
    }

    public void OnDisappearing()
    {
        IsBusy = false;
        IsLoading = false;
        // Clearing LinkedIn profile so that the previous value doesn't display during page loading
        _userService.ClearSocialMedia();
    }

    [RelayCommand]
    private async Task ComingSoon()
    {
        if (IsMe)
        {
            await CommunityToolkit.Maui.Alerts.Snackbar
                .Make("Coming soon. At that moment you can only add LinkedIn profile", duration: TimeSpan.FromSeconds(5))
                .Show();
        }
    }

    [RelayCommand]
    private async Task ClosePage()
    {
        await Navigation.PopModalAsync();
    }
}