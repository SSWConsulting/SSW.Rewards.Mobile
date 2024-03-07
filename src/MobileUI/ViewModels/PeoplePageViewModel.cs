using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Shared.DTOs.Staff;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class DevProfilesViewModel : BaseViewModel, IRecipient<PointsAwardedMessage>
{
    private IDevService _devService;
    private readonly ISnackbarService _snackbarService;

    public EventHandler<ScrollToEventArgs> ScrollToRequested;

    [ObservableProperty]
    bool isRunning;

    [ObservableProperty]
    bool twitterEnabled;

    [ObservableProperty]
    bool gitHubEnabled;

    [ObservableProperty]
    bool linkedinEnabled;

    [ObservableProperty]
    bool showDevCards;

    [ObservableProperty]
    bool scanned;

    [ObservableProperty]
    int points;

    public ObservableRangeCollection<DevProfile> Profiles { get; set; } = new(new List<DevProfile>(200));

    public ObservableCollection<StaffSkillDto> Skills { get; set; } = [];

    public DevProfile SelectedProfile { get; set; }

    [ObservableProperty]
    string devName;

    [ObservableProperty]
    string devTitle;

    [ObservableProperty]
    string devBio;

    [ObservableProperty]
    bool pageInView;

    public SnackbarOptions SnackOptions { get; set; }

    private string _twitterURI;
    private string _githubURI;
    private string _linkedinUri;
    private string _peopleUri;

    private int _lastProfileIndex;

    private bool _initialised;
    private bool _profilesLoaded;


    public DevProfilesViewModel(IDevService devService, ISnackbarService snackbarService)
    {
        TwitterEnabled = true;
        _devService = devService;
        _snackbarService = snackbarService;
        SnackOptions = new SnackbarOptions
        {
            Glyph = "\uf636",
            ActionCompleted = true,
            GlyphIsBrand = false,
            Message = "",
            Points = 500,
            ShowPoints = false
        };

        WeakReferenceMessenger.Default.Register(this);
    }

    public async Task Initialise()
    {
        if (!_initialised)
        {
            IsRunning = true;
            await LoadProfiles();
            _initialised = true;
            IsRunning = false;
        }
    }

    private async Task LoadProfiles()
    {
        var result = await _devService.GetProfilesAsync();
        var profiles = result.ToList();

        if (profiles.Count != 0)
        {
            _profilesLoaded = false;

            int i = 0;
            foreach (var profile in profiles)
            {
                profile.Index = i;
                i++;
            }

            Profiles.ReplaceRange(profiles);

            _lastProfileIndex = Profiles.Count - 1;
            ShowDevCards = true;
            _profilesLoaded = true;

            SetDevDetails();
        }
    }

    [RelayCommand]
    private void SetDevDetails()
    {
        if (_profilesLoaded)
        {
            if (SelectedProfile == null)
            {
                SelectedProfile = Profiles[0];
            }

            DevName = $"{SelectedProfile.FirstName} {SelectedProfile.LastName}";

            DevTitle = SelectedProfile.Title;

            DevBio = SelectedProfile.Bio;

            _twitterURI = "https://twitter.com/" + SelectedProfile.TwitterID;
            _githubURI = "https://github.com/" + SelectedProfile.GitHubID;
            _linkedinUri = "https://www.linkedin.com/in/" + SelectedProfile.LinkedInId;
            _peopleUri = GetPeopleUri(DevName);

            TwitterEnabled = !string.IsNullOrWhiteSpace(SelectedProfile.TwitterID);
            GitHubEnabled = !string.IsNullOrWhiteSpace(SelectedProfile.GitHubID);
            LinkedinEnabled = !string.IsNullOrWhiteSpace(SelectedProfile.LinkedInId);

            Scanned = SelectedProfile.Scanned;

            Points = SelectedProfile.Points;

            Skills.Clear();

            foreach (var skill in SelectedProfile.Skills.OrderByDescending(s => s.Level).Take(5))
            {
                Skills.Add(skill);
            }
        }
    }

    [RelayCommand]
    private void NavigateForward()
    {
        var selectedIndex = Profiles.IndexOf(SelectedProfile);

        if (selectedIndex < _lastProfileIndex)
        {
            var args = new ScrollToEventArgs { Index = ++selectedIndex };
            ScrollToRequested.Invoke(this, args);
        }
        else
        {
            var args = new ScrollToEventArgs { Index = 0 };
            ScrollToRequested.Invoke(this, args);
        }
    }

    [RelayCommand]
    private void NavigateBack()
    {
        var selectedIndex = Profiles.IndexOf(SelectedProfile);

        if (selectedIndex > 0)
        {
            var args = new ScrollToEventArgs { Index = --selectedIndex };
            ScrollToRequested.Invoke(this, args);
        }
        else
        {
            var args = new ScrollToEventArgs { Index = _lastProfileIndex };
            ScrollToRequested.Invoke(this, args);
        }
    }

    private string GetPeopleUri(string devname)
    {
        var profile = devname.Replace(' ', '-');

        profile = profile.TrimEnd('-').ToLower();

        return $"https://www.ssw.com.au/people/{profile}";
    }

    [RelayCommand]
    private async Task OpenTwitter()
    {
        if (TwitterEnabled)
        await Launcher.OpenAsync(new Uri(_twitterURI));
    }

    [RelayCommand]
    private async Task OpenGithub()
    {
        if (GitHubEnabled)
        await Launcher.OpenAsync(new Uri(_githubURI));
    }

    [RelayCommand]
    private async Task OpenLinkedin()
    {
        if (LinkedinEnabled)
        await Launcher.OpenAsync(new Uri(_linkedinUri));
    }

    [RelayCommand]
    private async Task OpenPeople()
    {
        await Launcher.OpenAsync(new Uri(_peopleUri));
    }

    [RelayCommand]
    private async void ShowScannedMessage()
    {
        var options = new SnackbarOptions
        {
            ActionCompleted = Scanned,
            Points = Points,
            Message = Scanned ? $"You've scanned {DevName}" : $"You haven't scanned {DevName} yet",
            ShowPoints = false,
            Glyph = "\uf636"
        };

        await _snackbarService.ShowSnackbar(options);
    }

    public void Receive(PointsAwardedMessage message)
    {
        _initialised = false;
    }

    [RelayCommand]
    private void SearchText(string searchBarText)
    {
        // UserStoppedTypingBehavior fires the command on a threadPool thread
        // as internally it uses .ContinueWith
        App.Current.MainPage.Dispatcher.Dispatch(() =>
        {
            var searchResult = Profiles.FirstOrDefault(x =>
                x.FirstName?.ToLower().Contains(searchBarText.ToLower()) == true ||
                x.LastName?.ToLower().Contains(searchBarText.ToLower()) == true
            );
            if (searchResult != null)
            {
                var args = new ScrollToEventArgs { Index = searchResult.Index, Animate = false };
                ScrollToRequested?.Invoke(this, args);
            }
        });
    }
}

public class ScrollToEventArgs : EventArgs
{
    public int Index { get; set; }
    public ScrollToPosition Position { get; set; } = ScrollToPosition.MakeVisible;
    public bool Animate { get; set; } = true;
}
