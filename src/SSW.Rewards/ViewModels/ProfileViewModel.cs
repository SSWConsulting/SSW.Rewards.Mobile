using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.PopupPages;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SSW.Rewards.Mobile.ViewModels;

public class ProfileViewModel : BaseViewModel, 
    IRecipient<ProfilePicUpdatedMessage>, 
    IRecipient<PointsAwardedMessage>, 
    IRecipient<AchievementTappedMessage>, 
    IRecipient<SocialUsernameAddedMessage>
{
    private readonly IRewardService _rewardsService;
    private IUserService _userService;

    public string ProfilePic { get; set; }
    public string Name { get; set; }
    public int Points { get; set; }
    public int Balance { get; set; }

    public bool ShowBalance { get; set; } = true;

    public double Progress { get; set; } = 0;

    private int userId { get; set; }

    public bool IsLoading { get; set; }

    public bool ShowCamera => _isMe && !IsLoading;

    public ObservableCollection<ProfileCarouselViewModel> ProfileSections { get; set; } = new ObservableCollection<ProfileCarouselViewModel>();

    public SnackbarOptions SnackOptions { get; set; }

    public ICommand CameraCommand => new Command(async () => await ShowCameraPageAsync());

    public ICommand PopProfile => new Command(async () => await Navigation.PopModalAsync());

    public bool ShowPopButton { get; set; } = false;

    private bool _isMe;

    private double _topRewardCost;

    private bool _loadingProfileSections;

    public EventHandler<ShowSnackbarEventArgs> ShowSnackbar;

    public ProfileViewModel(IRewardService rewardsService, IUserService userService)
    {
        WeakReferenceMessenger.Default.RegisterAll(this);

        IsLoading = true;
        RaisePropertyChanged("IsLoading");
        _rewardsService = rewardsService;
        _userService = userService;

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

    public ProfileViewModel(IRewardService rewardsService, IUserService userService, LeaderViewModel vm)
    {
        IsLoading = true;
        RaisePropertyChanged("IsLoading");
        ProfilePic = vm.ProfilePic;
        Name = vm.Name;
        userId = vm.UserId;
        Points = vm.TotalPoints;
        _userService = userService;
        _rewardsService = rewardsService;
        Balance = vm.Balance;
        ShowBalance = false;
        ShowPopButton = true;
    }

    public async Task Initialise(bool me)
    {        
        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            ProfileSections = new ObservableCollection<ProfileCarouselViewModel>();
            OnPropertyChanged(nameof(ProfileSections));
        }

        _isMe = me;

        if (_isMe)
        {
            var profilePic = _userService.MyProfilePic;

            //initialise me
            ProfilePic = profilePic;
            Name = _userService.MyName;
            Points = _userService.MyPoints;
            Balance = _userService.MyBalance;
            userId = _userService.MyUserId;
        }

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

        OnPropertyChanged(nameof(Progress));

        if (!ProfileSections.Any())
        {
            await LoadProfileSections();
        }

        IsLoading = false;

        RaisePropertyChanged(nameof(IsLoading), nameof(Name), nameof(ProfilePic), nameof(Points), nameof(Balance), nameof(ShowCamera));
    }

    private async Task OnPointsAwarded()
    {
        await _userService.UpdateMyDetailsAsync();
        await UpdatePoints();
        await LoadProfileSections();
    }

    private Task UpdatePoints()
    {
        Points = _userService.MyPoints;
        Balance = _userService.MyBalance;

        double progress = Balance / _topRewardCost;

        // TODO: we can get rid of this 0 condition if we award a 'sign up'
        // achievement. We could also potentially get the ring to render
        // empty.
        if (progress == 0)
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

        RaisePropertyChanged(nameof(Balance), nameof(Points), nameof(Progress));

        return Task.CompletedTask;
    }

    private async Task ShowCameraPageAsync()
    {
        var popup = new CameraPage(new CameraPageViewModel(_userService));
        //App.Current.MainPage.ShowPopup(popup);
        await MopupService.Instance.PushAsync(popup);
    }

    private async Task LoadProfileSections()
    {
        if (_loadingProfileSections)
            return;

        _loadingProfileSections = true;

        ProfileSections.Clear();

        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            ProfileSections = new ObservableCollection<ProfileCarouselViewModel>();
            OnPropertyChanged(nameof(ProfileSections));
        }

        var rewardList = await _userService.GetRewardsAsync(userId);
        var profileAchievements = await _userService.GetProfileAchievementsAsync(userId);
        var achievementList = await _userService.GetAchievementsAsync(userId);

        //===== Achievements =====

        var achivementsSection = new ProfileCarouselViewModel();
        achivementsSection.Type = CarouselType.Achievements;

        foreach (var achievement in profileAchievements)
        {
            achivementsSection.Achievements.Add(achievement.ToProfileAchievement());
        }

        ProfileSections.Add(achivementsSection);

        // ===== Recent activity =====

        var activitySection = new ProfileCarouselViewModel();
        activitySection.Type = CarouselType.RecentActivity;
        activitySection.IsMe = _isMe;
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

        Console.WriteLine($"[ProfileViewModel] Recent activity count: {activityList.Count}");

        ProfileSections.Add(activitySection);

        // ===== Notifications =====

        if (_isMe)
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

        _loadingProfileSections = false;
    }

    public void Receive(ProfilePicUpdatedMessage message)
    {
        ProfilePic = message.ProfilePic;
        RaisePropertyChanged(nameof(ProfilePic));
    }

    private void ProcessAchievement(ProfileAchievement achievement)
    {
        if (achievement.Complete)
        {
            ShowAchievementSnackbar(achievement);
        }
        else
        {
            if (achievement.Type == AchievementType.Linked)
            {
                var popup = new LinkSocial(achievement);
                MopupService.Instance.PushAsync(popup);
                //App.Current.MainPage.ShowPopup(popup);
            }
            else
            {
                ShowAchievementSnackbar(achievement);
            }
        }
    }

    private async Task AddSocialMediaId(SocialUsernameAddedMessage message)
    {
        WeakReferenceMessenger.Default.Unregister<SocialUsernameAddedMessage>(this);

        IsBusy = true;

        var result = await _userService.SaveSocialMediaId(message.Value.Achievement.Id, message.Value.Username);

        var options = new SnackbarOptions
        {
            ActionCompleted = false,
            Points = message.Value.Achievement.Value,
            GlyphIsBrand = message.Value.Achievement.IconIsBranded,
            Glyph = (string)typeof(Icon).GetField(message.Value.Achievement.AchievementIcon.ToString()).GetValue(null)
        };

        if (result)
        {
            options.ActionCompleted = true;
            message.Value.Achievement.Complete = true;
        }

        options.Message = $"{GetMessage(message.Value.Achievement)}";

        var args = new ShowSnackbarEventArgs { Options = options };

        ShowSnackbar.Invoke(this, args);

        if (result)
        {
            WeakReferenceMessenger.Default.Send(new PointsAwardedMessage());
        }

        IsBusy = false;
    }

    private void ShowAchievementSnackbar(ProfileAchievement achievement)
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

        var args = new ShowSnackbarEventArgs { Options = options };

        ShowSnackbar.Invoke(this, args);
    }

    public string GetMessage(Achievement achievement, bool IsActivity = false)
    {
        string prefix = _isMe ? "You have " : $"{Name} has ";

        if (!achievement.Complete)
        {
            prefix += "not ";
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
    }

    public async void Receive(PointsAwardedMessage message)
    {
        await OnPointsAwarded();
    }

    public void Receive(AchievementTappedMessage message)
    {
        ProcessAchievement(message.Value);
    }

    public void Receive(SocialUsernameAddedMessage message)
    {
        AddSocialMediaId(message);
    }
}