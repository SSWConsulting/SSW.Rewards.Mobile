using SSW.Rewards.Mobile.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public class DevProfilesViewModel : BaseViewModel, IRecipient<PointsAwardedMessage>
{
    private IDevService _devService;

    public ICommand OnCardSwiped => new Command(SetDevDetails);
    public ICommand StaffQRCommand => new Command(ShowScannedMessage);
    public ICommand OnTwitterTapped => new Command(async () => await OpenTwitter());
    public ICommand OnGithubTapped => new Command(async () => await OpenGithub());
    public ICommand OnLinkedinTapped => new Command(async () => await OpenLinkedin());
    public ICommand PeopleCommand => new Command(async () => await OpenPeople());
    public ICommand ForwardCommand => new Command(NavigateForward);
    public ICommand BackCommand => new Command(NavigateBack);
    public ICommand SearchTextCommand => new Command<string>(SearchTextHandler);

    public EventHandler<ScrollToEventArgs> ScrollToRequested;

    public EventHandler<ShowSnackbarEventArgs> ShowSnackbar;

    public bool IsRunning { get; set; }

    public bool TwitterEnabled { get; set; }
    public bool GitHubEnabled { get; set; }
    public bool LinkedinEnabled { get; set; }

    public bool ShowDevCards { get; set; } = false;

    public bool Scanned { get; set; } = false;

    public int Points { get; set; }

    public ObservableCollection<DevProfile> Profiles { get; set; } = new ObservableCollection<DevProfile>();

    public ObservableCollection<StaffSkillDto> Skills { get; set; } = new ObservableCollection<StaffSkillDto>();

    public DevProfile SelectedProfile { get; set; }

    public string DevName { get; set; }
    public string DevTitle { get; set; }
    public string DevBio { get; set; }

    public bool PageInView { get; set; } = false;

    public SnackbarOptions SnackOptions { get; set; }

    private string _twitterURI;
    private string _githubURI;
    private string _linkedinUri;
    private string _peopleUri;

    private int _lastProfileIndex;

    private bool _initialised = false;

    private bool _firstRun = true;

    public string[] OnSwipedUpdatePropertyList { get; set; }

    public DevProfilesViewModel(IDevService devService)
    {
        IsRunning = true;
        TwitterEnabled = true;
        _devService = devService;
        SnackOptions = new SnackbarOptions
        {
            Glyph = "\uf636",
            ActionCompleted = true,
            GlyphIsBrand = false,
            Message = "",
            Points = 500,
            ShowPoints = false
        };
        OnSwipedUpdatePropertyList = new [] { nameof(Title), nameof(DevName), nameof(DevTitle), nameof(DevBio), nameof(TwitterEnabled), nameof(GitHubEnabled), nameof(LinkedinEnabled), nameof(Scanned), nameof(Points) };

        WeakReferenceMessenger.Default.Register(this);
    }

    public async Task Initialise()
    {
        if (_firstRun)
        {
            await LoadProfiles();
            _firstRun = false;
        }
    }

    private async Task LoadProfiles()
    {
        var profiles = await _devService.GetProfilesAsync();

        if (profiles.Any())
        {
            int i = 0;
            foreach (var profile in profiles)
            {
                profile.Index = i;
                Profiles.Add(profile);
                i++;
            }

            IsRunning = false;

            _lastProfileIndex = Profiles.Count - 1;

            //SelectedProfile = Profiles[_lastProfileIndex];
            //OnPropertyChanged(nameof(SelectedProfile));

            ShowDevCards = true;
            OnPropertyChanged(nameof(ShowDevCards));

            // Disabling the scrolling carousel effect because
            // it's janky and probably not needed for v2 (it was
            // a v1 issue). Leacving this here becase we can 
            // probably fix the jankiness and re-enable for a nice
            // effect when we have time.

            //for (int i = _lastProfileIndex; i > -1; i--)
            //{
            //    if (PageInView)
            //    {
            //        ScrollToRequested.Invoke(this, i);
            //        await Task.Delay(50);
            //    }
            //}

            _initialised = true;

            SetDevDetails();

            RaisePropertyChanged(nameof(IsRunning));
        }
    }

    private void SetDevDetails()
    {
        if (_initialised)
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

            RaisePropertyChanged(OnSwipedUpdatePropertyList);

            Skills.Clear();

            foreach (var skill in SelectedProfile.Skills.OrderByDescending(s => s.Level).Take(5))
            {
                Skills.Add(skill);
            }
        }
    }

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

    private async Task OpenTwitter()
    {
        if (TwitterEnabled)
        await Launcher.OpenAsync(new Uri(_twitterURI));
    }

    private async Task OpenGithub()
    {
        if (GitHubEnabled)
        await Launcher.OpenAsync(new Uri(_githubURI));
    }

    private async Task OpenLinkedin()
    {
        if (LinkedinEnabled)
        await Launcher.OpenAsync(new Uri(_linkedinUri));
    }

    private async Task OpenPeople()
    {
        await Launcher.OpenAsync(new Uri(_peopleUri));
    }

    private void ShowScannedMessage()
    {
        var options = new SnackbarOptions
        {
            ActionCompleted = Scanned,
            Points = Points,
            Message = Scanned ? $"You've scanned {DevName}" : $"You haven't scanned {DevName} yet",
            ShowPoints = false,
            Glyph = "\uf636"
        };

        var args = new ShowSnackbarEventArgs { Options = options };

        ShowSnackbar.Invoke(this, args);
    }

    public async void Receive(PointsAwardedMessage message)
    {
        await LoadProfiles();
    }

    private void SearchTextHandler(string searchBarText)
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
