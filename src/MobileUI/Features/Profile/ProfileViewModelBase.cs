using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.PopupPages;
using SSW.Rewards.PopupPages;
using SSW.Rewards.Shared.DTOs.Staff;
using SSW.Rewards.Shared.DTOs.Users;
using SSW.Rewards.Shared.Utils;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class ProfileViewModelBase : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly IDevService _devService;
    private readonly IServiceProvider _provider;
    private readonly IFileCacheService _fileCacheService;

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
    [NotifyPropertyChangedFor(nameof(HasAnySocialMedia))]
    private string _linkedInUrl;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAnySocialMedia))]
    private string _gitHubUrl;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAnySocialMedia))]
    private string _twitterUrl;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAnySocialMedia))]
    private string _companyUrl;

    public bool HasAnySocialMedia =>
        !string.IsNullOrWhiteSpace(LinkedInUrl) ||
        !string.IsNullOrWhiteSpace(GitHubUrl) ||
        !string.IsNullOrWhiteSpace(TwitterUrl) ||
        !string.IsNullOrWhiteSpace(CompanyUrl);

    public bool ShowBalance { get; set; } = true;

    protected int UserId { get; set; }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isMe;

    public ObservableRangeCollection<Activity> RecentActivity { get; } = [];
    public ObservableRangeCollection<Activity> LastSeen { get; } = [];
    public ObservableRangeCollection<StaffSkillDto> Skills { get; set; } = [];
    private readonly SemaphoreSlim _loadingProfileSectionsSemaphore = new(1,1);

    public ProfileViewModelBase(
        bool isMe,
        IUserService userService,
        IDevService devService,
        IServiceProvider provider,
        IFileCacheService fileCacheService)
    {
        IsMe = isMe;
        _userService = userService;
        _devService = devService;
        _provider = provider;
        _fileCacheService = fileCacheService;
    }

    protected async Task _initialise()
    {
        await LoadProfileSections();
    }

    protected async Task LoadProfileSections()
    {
        if (!await _loadingProfileSectionsSemaphore.WaitAsync(0))
            return;

        try
        {
            IsLoading = true;

            string cacheKey = $"profile_{UserId}";

            await _fileCacheService.FetchAndRefresh(
                cacheKey,
                FetchProfileData,
                OnProfileDataReceived,
                this
            );
        }
        catch (Exception)
        {
            // Only show error if we never got any data (cached or fresh)
            if (IsLoading)
            {
                await ClosePage();
                await Shell.Current.DisplayAlert("Oops...", "There was an error loading this profile", "OK");
            }
        }
        finally
        {
            _loadingProfileSectionsSemaphore.Release();
            IsLoading = false;
        }
    }

    private async Task<CachedProfileData> FetchProfileData()
    {
        var profileTask = _userService.GetUserAsync(UserId);
        var socialMediaTask = _userService.GetSocialMedia(UserId);

        await Task.WhenAll(profileTask, socialMediaTask);

        var profile = profileTask.Result;
        var socialMedia = socialMediaTask.Result;

        // Get skills if staff member
        List<StaffSkillDto> skills = [];
        if (profile.IsStaff)
        {
            try
            {
                DevProfile devProfile = await _devService.GetProfileAsync(profile.Email);
                if (devProfile != null)
                {
                    skills = devProfile.Skills.OrderByDescending(s => s.Level).Take(3).ToList();
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the whole operation
                System.Diagnostics.Debug.WriteLine($"Failed to load skills: {ex.Message}");
            }
        }

        return new CachedProfileData
        {
            ProfilePic = profile.ProfilePic,
            FullName = profile.FullName,
            Rank = profile.Rank,
            Points = profile.Points,
            Balance = profile.Balance,
            IsStaff = profile.IsStaff,
            Email = profile.Email,
            Achievements = profile.Achievements.ToList(),
            Rewards = profile.Rewards.ToList(),
            Skills = skills,
            LinkedInUrl = GetSocialMediaUrl(socialMedia, Constants.SocialMediaPlatformIds.LinkedIn),
            GitHubUrl = GetSocialMediaUrl(socialMedia, Constants.SocialMediaPlatformIds.GitHub),
            TwitterUrl = GetSocialMediaUrl(socialMedia, Constants.SocialMediaPlatformIds.Twitter),
            CompanyUrl = GetSocialMediaUrl(socialMedia, Constants.SocialMediaPlatformIds.Company)
        };
    }

    private async Task OnProfileDataReceived(CachedProfileData profileData, bool isFromCache, object tag)
    {
        // Only process if this callback is for the current instance
        if (tag != this)
            return;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            // Stop loading as soon as we get cached data
            if (isFromCache && IsLoading)
            {
                IsLoading = false;
            }

            ProfilePic = profileData.ProfilePic ?? "v2sophie";
            Name = profileData.FullName ?? string.Empty;
            Rank = profileData.Rank;
            Points = profileData.Points;
            Balance = profileData.Balance;
            IsStaff = profileData.IsStaff;
            UserEmail = profileData.Email ?? string.Empty;
            Title = GetTitle();
            LinkedInUrl = profileData.LinkedInUrl;
            GitHubUrl = profileData.GitHubUrl;
            TwitterUrl = profileData.TwitterUrl;
            CompanyUrl = profileData.CompanyUrl;

            UpdateSkillsSection(profileData.Skills);
            UpdateLastSeenSection(profileData.Achievements);
            UpdateRecentActivitySection(profileData.Achievements, profileData.Rewards);
        });
    }

    private static string GetSocialMediaUrl(List<UserSocialMediaIdDto> socialMediaList, int socialMediaPlatformId)
    {
        return socialMediaList.FirstOrDefault(x => x.SocialMediaPlatformId == socialMediaPlatformId)?.SocialMediaUserId;
    }

    private string GetTitle()
    {
        if (IsStaff)
        {
            return "SSW";
        }

        return !string.IsNullOrEmpty(CompanyUrl)
            ? RegexHelpers.WebsiteRegex().Replace(CompanyUrl, string.Empty)
            : "Community";
    }

    [RelayCommand]
    private void ChangeProfilePicture()
    {
        if (IsLoading || !IsMe)
            return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var vm = ActivatorUtilities.CreateInstance<ProfilePictureViewModel>(_provider);
            var popup = ActivatorUtilities.CreateInstance<ProfilePicturePage>(_provider, vm);
            await MopupService.Instance.PushAsync(popup);
        });
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

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var page = ActivatorUtilities.CreateInstance<AddSocialMediaPage>(_provider, socialMediaPlatformId, string.Empty);
                await MopupService.Instance.PushAsync(page);
            });

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
                await Shell.Current.DisplayAlert("Error", "There was an error trying to launch the default browser.", "OK");
            }
        }
    }

    private string GetMessage(UserAchievementDto achievement, bool isActivity = false)
    {
        string prefix = IsMe ? "You have" : $"{Name} has";

        if (!achievement.Complete)
        {
            prefix += " not";
        }

        string activity = achievement.AchievementName;
        string action;
        string scored = $"just scored {achievement.AchievementValue}pts for";

        switch (achievement.AchievementType)
        {
            case AchievementType.Attended:
                action = "checked into";
                break;

            case AchievementType.Linked:
                action = $"{scored} linking";
                activity = activity.Split(' ').Last();
                break;

            case AchievementType.Scanned:
                action = $"{scored} scanning";
                break;

            case AchievementType.Completed:
            default:
                action = $"{scored} completing";
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

    private void UpdateSkillsSection(IEnumerable<StaffSkillDto> skills)
    {
        Skills.ReplaceRange(skills);
    }

    private void UpdateLastSeenSection(IEnumerable<UserAchievementDto> achievementList)
    {
        var recentLastSeen = achievementList.Where(a => a.AchievementType == AchievementType.Attended)
            .OrderByDescending(a => a.AwardedAt).Take(5).Select(
                achievement => new Activity
            {
                ActivityName = GetMessage(achievement, true),
                OccurredAt = achievement.AwardedAt,
                Type = achievement.AchievementType.ToActivityType(),
                TimeElapsed = achievement.AwardedAt != null ? DateTimeHelpers.GetTimeElapsed((DateTime)achievement.AwardedAt) : string.Empty
            });

        LastSeen.ReplaceRange(recentLastSeen);
    }

    private void UpdateRecentActivitySection(IEnumerable<UserAchievementDto> achievements, IEnumerable<UserRewardDto> rewards)
    {
        const int takeSize = 5;
        List<Activity> activities = [];

        activities.AddRange(FilterRecentAchievements(achievements, takeSize));
        activities.AddRange(FilterRecentRewards(rewards, takeSize));

        var recentActivity = activities.OrderByDescending(a => a.OccurredAt).Take(takeSize);
        RecentActivity.ReplaceRange(recentActivity);
    }

    private List<Activity> FilterRecentAchievements(IEnumerable<UserAchievementDto> achievementList, int takeSize)
    {
        List<Activity> result = [];
        var recentAchievements = achievementList
            .Where(a => a.AchievementType != AchievementType.Attended)
            .OrderByDescending(a => a.AwardedAt)
            .Take(takeSize);

        result.AddRange(recentAchievements.Select(achievement => new Activity
        {
            ActivityName = GetMessage(achievement, true),
            OccurredAt = achievement.AwardedAt,
            Type = achievement.AchievementType.ToActivityType(),
            TimeElapsed = achievement.AwardedAt != null ? DateTimeHelpers.GetTimeElapsed((DateTime)achievement.AwardedAt) : string.Empty
        }));

        return result;
    }

    private static List<Activity> FilterRecentRewards(IEnumerable<UserRewardDto> rewardList, int takeSize)
    {
        List<Activity> result = [];
        var recentRewards = rewardList
            .Where(r => r.Awarded)
            .OrderByDescending(r => r.AwardedAt)
            .Take(takeSize);

        result.AddRange(recentRewards.Select(reward => new Activity
        {
            ActivityName = $"Claimed {reward.RewardName}",
            OccurredAt = reward.AwardedAt,
            Type = ActivityType.Claimed,
            TimeElapsed = reward.AwardedAt != null ? DateTimeHelpers.GetTimeElapsed((DateTime)reward.AwardedAt) : string.Empty
        }));

        return result;
    }

    public void OnDisappearing()
    {
        IsBusy = false;
        IsLoading = false;
        // Clearing LinkedIn profile so that the previous value doesn't display during page loading
        _userService.ClearSocialMedia();
    }

    [RelayCommand]
    private static async Task ClosePage()
    {
        await Shell.Current.GoToAsync("..");
    }
}

public class CachedProfileData
{
    public string ProfilePic { get; init; }
    public string FullName { get; init; }
    public int Rank { get; init; }
    public int Points { get; init; }
    public int Balance { get; init; }
    public bool IsStaff { get; init; }
    public string Email { get; init; }
    public string LinkedInUrl { get; init; }
    public string GitHubUrl { get; init; }
    public string TwitterUrl { get; init; }
    public string CompanyUrl { get; init; }
    public List<UserAchievementDto> Achievements { get; init; } = [];
    public List<UserRewardDto> Rewards { get; init; } = [];
    public List<StaffSkillDto> Skills { get; init; } = [];
}
