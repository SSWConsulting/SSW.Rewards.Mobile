using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.PopupPages;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class ProfileViewModelBase : BaseViewModel, IRecipient<AchievementTappedMessage>
{
    protected readonly IRewardService _rewardsService;
    protected IUserService _userService;
    protected readonly ISnackbarService _snackbarService;

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

    [ObservableProperty]
    private double _progress;

    protected int userId { get; set; }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isMe;
    
    [ObservableProperty]
    private bool _isLastSeenEmpty;
    
    [ObservableProperty]
    private bool _isRecentActivityEmpty;
    
    public ObservableCollection<Activity> RecentActivity { get; set; } = new ();
    
    public ObservableCollection<Activity> LastSeen { get; set; } = new ();

    public SnackbarOptions SnackOptions { get; set; }

    public ICommand CameraCommand => new Command(async () => await ShowCameraPageAsync());

    public ICommand PopProfile => new Command(async () => await Navigation.PopModalAsync());

    public bool ShowPopButton { get; set; } = false;

    protected double _topRewardCost;

    private readonly SemaphoreSlim _loadingProfileSectionsSemaphore = new(1,1);

    public ProfileViewModelBase(IRewardService rewardsService, IUserService userService, ISnackbarService snackbarService)
    {
        IsLoading = true;
        _rewardsService = rewardsService;
        _userService = userService;
        _snackbarService = snackbarService;


        SnackOptions = new SnackbarOptions
        {
            ActionCompleted = true,
            GlyphIsBrand = true,
            Glyph = "\uf420",
            Message = "You have completed the Angular quiz",
            Points = 1000,
            ShowPoints = true
        };
    }

    public void OnAppearing()
    {
        if (WeakReferenceMessenger.Default.IsRegistered<AchievementTappedMessage>(this))
            return;
        
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void Receive(AchievementTappedMessage message)
    {
        ProcessAchievement(message.Value);
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

        var topReward = rewards.OrderByDescending(r => r.Cost).First();

        _topRewardCost = (double)topReward.Cost;

        double progress = Balance / _topRewardCost;

        // TODO: we can get rid of this 0 condition if we award a 'sign up'
        // achievement. We could also potentially get the ring to render
        // empty.
        if (progress <= 0)
        {
            Progress = 0.01;
        }
        else if (progress < 1)
        {
            Progress = progress;
        }
        else
        {
            Progress = 1;
        }

        await LoadProfileSections();

        IsLoading = false;
    }

    private void ProcessAchievement(ProfileAchievement achievement)
    {
        if (achievement.IsMe == IsMe)
        {
            if (achievement.Complete)
            {
                ShowAchievementSnackbar(achievement);
            }
            else
            {
                if (achievement.Type == AchievementType.Linked && IsMe)
                {
                    var popup = new LinkSocial(achievement);
                    MopupService.Instance.PushAsync(popup);
                }
                else
                {
                    ShowAchievementSnackbar(achievement);
                }
            }
        }
    }

    private async Task ShowCameraPageAsync()
    {
        if (IsLoading)
            return;
        
        var popup = new CameraPage(new CameraPageViewModel(_userService));
        //App.Current.MainPage.ShowPopup(popup);
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
        
        // ===== Last seen =====
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

        // ===== Recent activity =====
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

        foreach (var activity in activityList.OrderByDescending(a => a.OccurredAt).Take(5))
        {
            RecentActivity.Add(activity);
        }
        
        IsLastSeenEmpty = !LastSeen.Any();
        IsRecentActivityEmpty = !RecentActivity.Any();

        _loadingProfileSectionsSemaphore.Release();
    }

    public void Receive(ProfilePicUpdatedMessage message)
    {
        ProfilePic = message.ProfilePic;
    }

    private async void ShowAchievementSnackbar(ProfileAchievement achievement)
    {
        // TODO: set Glyph when given values
        var options = new SnackbarOptions
        {
            ActionCompleted = achievement.Complete,
            Points = achievement.Value,
            Message = $"{GetMessage(achievement)}",
            GlyphIsBrand = achievement.IconIsBranded,
            Glyph = (string)typeof(Icon).GetField(achievement.AchievementIcon.ToString()).GetValue(null)
        };

        await _snackbarService.ShowSnackbar(options);
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
    
    private static string GetTimeElapsed(DateTime occurredAt)
    {
        return (DateTime.Now - occurredAt) switch
        {
            { TotalDays: < 1 } ts => $"{ts.Hours}h",
            { TotalDays: < 31 } ts => $"{ts.Days}d",
            _ => occurredAt.ToString("dd MMMM yyyy"),
        };
    }

    // The following method is required as a workaround
    // for a bug in Xamarin.Forms. See:
    // https://github.com/xamarin/Xamarin.Forms/issues/14952
    // for both the issue and the workaround.
    public void OnDisappearing()
    {
        IsBusy = false;
        IsLoading = false;
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}