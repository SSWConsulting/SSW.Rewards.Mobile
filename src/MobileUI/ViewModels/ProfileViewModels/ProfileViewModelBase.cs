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

    public bool ShowBalance { get; set; } = true;

    [ObservableProperty]
    private double _progress;

    protected int userId { get; set; }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isMe;
    
    public ObservableCollection<ProfileCarouselViewModel> ProfileSections { get; set; } = new ObservableCollection<ProfileCarouselViewModel>();

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

        if (!ProfileSections.Any())
        {
            await LoadProfileSections();
        }

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

        ProfileSections.Clear();
        
        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            ProfileSections = new ObservableCollection<ProfileCarouselViewModel>();
            OnPropertyChanged(nameof(ProfileSections));
        }

        var rewardListTask = _userService.GetRewardsAsync(userId);
        var profileAchievementsTask = _userService.GetProfileAchievementsAsync(userId);
        var achievementListTask = _userService.GetAchievementsAsync(userId);

        await Task.WhenAll(rewardListTask, profileAchievementsTask, achievementListTask);

        var rewardList = rewardListTask.Result;
        var profileAchievements = profileAchievementsTask.Result;
        var achievementList = achievementListTask.Result;


        //===== Achievements =====

        var achivementsSection = new ProfileCarouselViewModel();
        achivementsSection.Type = CarouselType.Achievements;

        foreach (var achievement in profileAchievements)
        {
            achivementsSection.Achievements.Add(achievement.ToProfileAchievement(IsMe));
        }

        ProfileSections.Add(achivementsSection);

        // ===== Recent activity =====

        var activitySection = new ProfileCarouselViewModel();
        activitySection.Type = CarouselType.RecentActivity;
        activitySection.IsMe = IsMe;
        activitySection.ProfileName = Name;

        var activityList = new List<Activity>();

        var recentAchievements = achievementList.OrderByDescending(a => a.AwardedAt).Take(10);

        foreach (var achievement in recentAchievements)
        {
            activityList.Add(new Activity
            {
                ActivityName = GetMessage(achievement, true),
                OcurredAt = achievement.AwardedAt,
                Type = achievement.Type.ToActivityType()
            });
        }

        var recentRewards = rewardList
            .Where(r => r.Awarded == true)
            .OrderByDescending(r => r.AwardedAt).Take(10);

        foreach (var reward in rewardList)
        {
            activityList.Add(new Activity
            {
                ActivityName = $"Claimed {reward.Name}",
                OcurredAt = reward.AwardedAt?.DateTime,
                Type = ActivityType.Claimed
            });
        }

        activityList.OrderByDescending(a => a.OcurredAt).Take(10).ToList().ForEach(a => activitySection.RecentActivity.Add(a));

        ProfileSections.Add(activitySection);

        // ===== Notifications =====

        if (IsMe)
        {
            var notificationsSection = new ProfileCarouselViewModel();
            notificationsSection.Type = CarouselType.Notifications;

#if DEBUG
            notificationsSection.Notifications.Add(new Notification
            {
                Message = "New tech trivia available: Scrum",
                Type = NotificationType.Alert
            });

            notificationsSection.Notifications.Add(new Notification
            {
                Message = "Upcoming event: SSW User Group June 2022 with Jason Taylor",
                Type = NotificationType.Event
            });

            notificationsSection.Notifications.Add(new Notification
            {
                Message = "Upcoming event: How to design darkmode mobile UI with Jayden Alchin",
                Type = NotificationType.Event
            });
#endif

            ProfileSections.Add(notificationsSection);
        }

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

    public string GetMessage(Achievement achievement, bool IsActivity = false)
    {
        string prefix = IsMe ? "You have" : $"{Name} has";

        if (!achievement.Complete)
        {
            prefix += " not";
        }


        string activity = achievement.Name;

        string action = string.Empty;

        switch (achievement.Type)
        {
            case AchievementType.Attended:
                action = "attended";
                break;

            case AchievementType.Completed:
                action = "completed";
                break;

            case AchievementType.Linked:
                action = "followed";
                activity = activity.Replace("follow", "");
                activity = activity.Replace("Follow", "");
                break;

            case AchievementType.Scanned:
                action = "scanned";
                break;
        }

        if (IsActivity)
        {
            action = char.ToUpper(action[0]) + action.Substring(1);
            return $"{action} {activity}";
        }
        else
        {
            return $"{prefix} {action} {activity}";
        }
    }

    // The following method is required as a workaround
    // for a bug in Xamarin.Forms. See:
    // https://github.com/xamarin/Xamarin.Forms/issues/14952
    // for both the issue and the workaround.
    public void OnDisappearing()
    {
        IsBusy = false;
        IsLoading = false;
        ProfileSections = new ObservableCollection<ProfileCarouselViewModel>();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}