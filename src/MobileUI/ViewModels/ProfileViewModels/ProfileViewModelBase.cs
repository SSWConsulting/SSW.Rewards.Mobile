using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.PopupPages;
using SSW.Rewards.PopupPages;
using SSW.Rewards.Shared.DTOs.Staff;

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

    protected int userId { get; set; }

    public bool ShowCloseButton { get; set; } = true;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isMe;

    [ObservableProperty]
    private Color _linkedInColor = Colors.White;

    public ObservableCollection<Activity> RecentActivity { get; } = [];
    public ObservableCollection<Activity> LastSeen { get; } = [];
    public ObservableCollection<StaffSkillDto> Skills { get; set; } = [];
    private readonly SemaphoreSlim _loadingProfileSectionsSemaphore = new(1,1);

    public ProfileViewModelBase(
        IRewardService rewardsService,
        IUserService userService,
        IDevService devService,
        IPermissionsService permissionsService,
        ISnackbarService snackbarService)
    {
        IsLoading = true;
        _rewardsService = rewardsService;
        _userService = userService;
        _devService = devService;
        _permissionsService = permissionsService;
        _snackbarService = snackbarService;
        userService.LinkedInProfileObservable().Subscribe(myLinkedIn =>
        {
            LinkedInUrl = myLinkedIn;
            App.Current.Resources.TryGetValue("SSWRed", out object sswRed);
            LinkedInColor = (string.IsNullOrWhiteSpace(myLinkedIn) ? Colors.White : (Color)sswRed)!;
        });
    }

    protected async Task _initialise()
    {
        IsLoading = true;
        var rewards = await _rewardsService.GetRewards(); // TODO: do we need this?
        await LoadProfileSections();
        IsLoading = false;
    }

    protected async Task LoadProfileSections()
    {
        if (!_loadingProfileSectionsSemaphore.Wait(0))
            return;

        var rewardListTask = _userService.GetRewardsAsync(userId);
        var achievementListTask = _userService.GetAchievementsAsync(userId);
        var loadSocialMediaTask = LoadSocialMedia();
        await Task.WhenAll(rewardListTask, achievementListTask, loadSocialMediaTask);

        var rewardList = rewardListTask.Result;
        var achievementList = achievementListTask.Result;

        UpdateLastSeenSection(achievementList);
        UpdateRecentActivitySection(achievementList, rewardList);
        await UpdateSkillsSectionIfRequired();

        _loadingProfileSectionsSemaphore.Release();
    }

    private async Task LoadSocialMedia()
    {
        var linkedInAchievementId = 2; // LinkedIn Achievement
        await _userService.LoadSocialMedia(userId, linkedInAchievementId);
    }

    [RelayCommand]
    private async Task ChangeProfilePicture()
    {
        if (IsLoading)
            return;

        var popup = new CameraPage(new CameraPageViewModel(_userService, _permissionsService));
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

        var uri = new Uri(LinkedInUrl);
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }

    public string GetMessage(Achievement achievement, bool isActivity = false)
    {
        string prefix = IsMe ? "You have" : $"{Name} has";

        if (!achievement.Complete)
        {
            prefix += " not";
        }

        string activity = achievement.Name;
        string action = string.Empty;
        string scored = $"just scored {achievement.Value}pts for";

        switch (achievement.Type)
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

    private void UpdateLastSeenSection(IEnumerable<Achievement> achievementList)
    {
        LastSeen.Clear(); // it could contain data from another user profile
        var recentLastSeen = achievementList.Where(a => a.Type == AchievementType.Attended).OrderByDescending(a => a.AwardedAt).Take(5);
        foreach (var achievement in recentLastSeen)
        {
            LastSeen.Add(new Activity
            {
                ActivityName = GetMessage(achievement, true),
                OccurredAt = achievement.AwardedAt,
                Type = achievement.Type.ToActivityType(),
                TimeElapsed = GetTimeElapsed(achievement.AwardedAt.Value)
            });
        }
    }

    private void UpdateRecentActivitySection(IEnumerable<Achievement> achievementList, IEnumerable<Reward> rewardList)
    {
        var activityList = new List<Activity>();
        var recentAchievements = achievementList.Where(a => a.Type != AchievementType.Attended).OrderByDescending(a => a.AwardedAt).Take(5);
        foreach (var achievement in recentAchievements)
        {
            activityList.Add(new Activity
            {
                ActivityName = GetMessage(achievement, true),
                OccurredAt = achievement.AwardedAt,
                Type = achievement.Type.ToActivityType(),
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
                ActivityName = $"Claimed {reward.Name}",
                OccurredAt = reward.AwardedAt?.DateTime,
                Type = ActivityType.Claimed,
                TimeElapsed = GetTimeElapsed((DateTime)reward.AwardedAt?.DateTime)
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
        return (DateTime.Now - occurredAt) switch
        {
            { TotalDays: < 1 } ts => $"{ts.Hours}h",
            { TotalDays: < 31 } ts => $"{ts.Days}d",
            _ => occurredAt.ToString("dd MMMM yyyy"),
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