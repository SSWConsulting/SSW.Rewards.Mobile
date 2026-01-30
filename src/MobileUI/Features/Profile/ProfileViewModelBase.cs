using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ProfileViewModelBase> _logger;
    private readonly IAlertService _alertService;

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

    private string _cacheTag;

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

    public ObservableRangeCollection<Activity> Activity { get; set; } = [];
    public ObservableRangeCollection<StaffSkillDto> Skills { get; set; } = [];

    private readonly SemaphoreSlim _loadingProfileSectionsSemaphore = new(1, 1);

    private const int MaxActivity = 10;
    private const int MaxSkills = 3;
    private const string DefaultProfilePic = "v2sophie";

    public ProfileViewModelBase(
        bool isMe,
        IUserService userService,
        IDevService devService,
        IServiceProvider provider,
        IFileCacheService fileCacheService,
        IAlertService alertService,
        ILogger<ProfileViewModelBase> logger)
    {
        IsMe = isMe;
        _userService = userService;
        _devService = devService;
        _provider = provider;
        _fileCacheService = fileCacheService;
        _logger = logger;
        _alertService = alertService;
    }

    protected async Task _initialise()
    {
        await LoadProfileSections();
    }

    protected async Task LoadProfileSections()
    {
        if (!await _loadingProfileSectionsSemaphore.WaitAsync(0))
            return;

        bool hasCachedData = false;

        try
        {
            IsLoading = true;

            string cacheKey = $"profile_{UserId}";
            _cacheTag = DateTime.UtcNow.Ticks.ToString();

            await _fileCacheService.FetchAndRefresh(
                cacheKey,
                FetchProfileData,
                (data, isFromCache, tag) =>
                {
                    hasCachedData = isFromCache;
                    return OnProfileDataReceived(data, isFromCache, tag);
                },
                _cacheTag
            );
        }
        catch (Exception ex)
        {
            // Only show error if we never got any cached data
            if (!hasCachedData)
            {
                _logger.LogError("Profile loading error: {Message}", ex.Message);
                await _alertService.DisplayAlertAsync("Oops...", "There was an error loading this profile", "OK");
                await ClosePage();
            }
            else
            {
                // Log the error but don't interrupt the user since they have cached data
                _logger.LogInformation("Profile refresh failed but cached data available: {Message}", ex.Message);
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
        var profile = await _userService.GetUserAsync(UserId);
        var socialMedia = await _userService.GetSocialMedia(UserId);

        // Get skills if staff member
        List<StaffSkillDto> skills = [];
        if (profile.IsStaff)
        {
            try
            {
                DevProfile devProfile = await _devService.GetProfileAsync(profile.Email);
                if (devProfile != null)
                {
                    skills = devProfile.Skills.OrderByDescending(s => s.Level).Take(MaxSkills).ToList();
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the whole operation
                _logger.LogError(ex, "Failed to load skills: {Message}", ex.Message);
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
        if (!Equals(tag, _cacheTag))
            return;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            // Stop loading as soon as we get cached data
            if (isFromCache && IsLoading)
            {
                IsLoading = false;
            }

            ProfilePic = profileData.ProfilePic ?? DefaultProfilePic;
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
            UpdateActivitySection(profileData.Achievements, profileData.Rewards);
        });
    }

    private static string GetSocialMediaUrl(List<UserSocialMediaIdDto> socialMediaList, int socialMediaPlatformId)
    {
        return socialMediaList?.FirstOrDefault(x => x.SocialMediaPlatformId == socialMediaPlatformId)
            ?.SocialMediaUserId;
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
    private async Task ChangeProfilePicture()
    {
        if (IsLoading || !IsMe)
            return;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                var vm = ActivatorUtilities.CreateInstance<ProfilePictureViewModel>(_provider);
                var popup = ActivatorUtilities.CreateInstance<ProfilePicturePage>(_provider, vm);
                await MopupService.Instance.PushAsync(popup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open profile picture page");
                await _alertService.DisplayAlertAsync("Error", "There was an error trying to open the popup.", "OK");
            }
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

    [RelayCommand]
    private static async Task GoToScanPage()
    {
        await Shell.Current.GoToAsync("//scan");
    }

    private async Task OpenProfile(string userProfile, int socialMediaPlatformId)
    {
        if (string.IsNullOrWhiteSpace(userProfile))
        {
            if (!IsMe)
            {
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var page = ActivatorUtilities.CreateInstance<AddSocialMediaPage>(_provider, socialMediaPlatformId,
                        string.Empty);
                    await MopupService.Instance.PushAsync(page);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open add social media popup");
                    await _alertService.DisplayAlertAsync("Error", "There was an error trying to open the popup.", "OK");
                }
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
                await _alertService.DisplayAlertAsync("Error", "There was an error trying to launch the default browser.",
                    "OK");
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
        string scored = $"scored {achievement.AchievementValue}pts for";

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

    private void UpdateActivitySection(List<UserAchievementDto> achievements, List<UserRewardDto> rewards)
    {
        var allActivities = new List<Activity>();

        if (achievements?.Count > 0)
        {
            allActivities.AddRange(achievements.Select(CreateActivityFromAchievement));
        }

        if (rewards?.Count > 0)
        {
            allActivities.AddRange(rewards.Select(CreateActivityFromReward));
        }

        var sortedActivities = allActivities
            .OrderByDescending(x => x.OccurredAt)
            .Take(MaxActivity)
            .ToList();

        Activity.ReplaceRange(sortedActivities);
    }

    private Activity CreateActivityFromAchievement(UserAchievementDto achievement) => new()
    {
        ActivityName = GetMessage(achievement, true),
        OccurredAt = achievement.AwardedAt,
        Type = achievement.AchievementType.ToActivityType(),
        TimeElapsed = achievement.AwardedAt.HasValue
            ? DateTimeHelpers.GetTimeElapsed(achievement.AwardedAt.Value)
            : string.Empty
    };

    private static Activity CreateActivityFromReward(UserRewardDto reward) => new()
    {
        ActivityName = $"Claimed {reward.RewardName}",
        OccurredAt = reward.AwardedAt,
        Type = ActivityType.Claimed,
        TimeElapsed = reward.AwardedAt.HasValue
            ? DateTimeHelpers.GetTimeElapsed(reward.AwardedAt.Value)
            : string.Empty
    };

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