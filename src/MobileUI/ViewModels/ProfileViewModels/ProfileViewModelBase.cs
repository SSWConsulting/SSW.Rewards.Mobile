using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.PopupPages;
using SSW.Rewards.Shared.DTOs.Staff;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class ProfileViewModelBase : BaseViewModel
{
    private readonly IRewardService _rewardsService;
    private readonly IUserService _userService;
    private readonly IDevService _devService;
    private readonly IPermissionsService _permissionsService;

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

    public bool ShowBalance { get; set; } = true;

    protected int userId { get; set; }
    
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
        IRewardService rewardsService,
        IUserService userService,
        IDevService devService,
        IPermissionsService permissionsService)
    {
        IsLoading = true;
        _rewardsService = rewardsService;
        _userService = userService;
        _devService = devService;
        _permissionsService = permissionsService;
    }

    protected async Task _initialise()
    {
        var rewards = await _rewardsService.GetRewards();

        // TODO: If there is an authentication failure, the RewardsService
        // will pop a modal login page so the user can reauthenticate. In
        // that case, do nothing here. We can remove this check when we
        // implement Polly instead to make this process more robust. See
        // https://github.com/SSWConsulting/SSW.Rewards/issues/276
        if (!rewards.Any())
            return;

        await LoadProfileSections();

        IsLoading = false;
    }

    [RelayCommand]
    private async Task ChangeProfilePicture()
    {
        if (IsLoading)
            return;

        var popup = new CameraPage(new CameraPageViewModel(_userService, _permissionsService));
        await MopupService.Instance.PushAsync(popup);
    }

    protected async Task LoadProfileSections()
    {
        if (!_loadingProfileSectionsSemaphore.Wait(0))
            return;

        var rewardListTask = _userService.GetRewardsAsync(userId);
        var achievementListTask = _userService.GetAchievementsAsync(userId);
        await Task.WhenAll(rewardListTask, achievementListTask);

        var rewardList = rewardListTask.Result;
        var achievementList = achievementListTask.Result;

        UpdateLastSeenSection(achievementList);
        UpdateRecentActivitySection(achievementList, rewardList);
        await UpdateSkillsSectionIfRequired();

        _loadingProfileSectionsSemaphore.Release();
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
    }
    
    [RelayCommand]
    private async Task ClosePage()
    {
        await Navigation.PopModalAsync();
    }
}