using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SSW.Rewards.Models;
using SSW.Rewards.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class DevProfilesViewModel : BaseViewModel
    {
        private IDevService _devService;
        private readonly IUserService _userService;

        public ICommand OnCardSwiped => new Command(SetDevDetails);

        public ICommand OnTwitterTapped => new Command(async () => await OpenTwitter());
        public ICommand OnGithubTapped => new Command(async () => await OpenGithub());
        public ICommand OnLinkedinTapped => new Command(async () => await OpenLinkedin());

        public ICommand ForwardCommand => new Command(NavigateForward);
        public ICommand BackCommand => new Command(NavigateBack);

        public EventHandler<int> ScrollToRequested;

        public bool IsRunning { get; set; }

        public bool TwitterEnabled { get; set; }
        public bool GitHubEnabled { get; set; }
        public bool LinkedinEnabled { get; set; }

        public bool ShowDevCards { get; set; } = false;

        public ObservableCollection<DevProfile> Profiles { get; set; } = new ObservableCollection<DevProfile>();

        public ObservableCollection<StaffSkillDto> Skills { get; set; } = new ObservableCollection<StaffSkillDto>();

        public DevProfile SelectedProfile { get; set; }

        public string DevName { get; set; }
        public string DevTitle { get; set; }
        public string DevBio { get; set; }

        private string _twitterURI;
        private string _githubURI;
        private string _linkedinUri;

        private int _lastProfileIndex;

        private bool _initialised = false;

        public string[] OnSwipedUpdatePropertyList { get; set; }

        public DevProfilesViewModel(IDevService devService)
        {
            IsRunning = true;
            TwitterEnabled = true;
            _devService = devService;
            OnSwipedUpdatePropertyList = new string[] { nameof(Title), nameof(DevName), nameof(DevTitle), nameof(DevBio), nameof(TwitterEnabled), nameof(GitHubEnabled), nameof(LinkedinEnabled) };
        }

        public async Task Initialise()
        {
            var profiles = await _devService.GetProfilesAsync();

            foreach(var profile in profiles)
            {
                Profiles.Add(profile);
            }

            IsRunning = false;

            _initialised = true;

            _lastProfileIndex = Profiles.Count - 1;

            //ScrollToRequested.Invoke(this, _lastProfileIndex);

            SelectedProfile = Profiles[_lastProfileIndex];
            OnPropertyChanged(nameof(SelectedProfile));

            ShowDevCards = true;
            OnPropertyChanged(nameof(ShowDevCards));

            for (int i = _lastProfileIndex; i > -1; i--)
            {
                ScrollToRequested.Invoke(this, i);
                await Task.Delay(50);
            }

            SetDevDetails();

            RaisePropertyChanged(nameof(IsRunning));
        }

        private void SetDevDetails()
        {
            if (_initialised)
            {
                try
                {
                    DevName = $"{SelectedProfile.FirstName} {SelectedProfile.LastName}";

                    DevTitle = SelectedProfile.Title;

                    DevBio = SelectedProfile.Bio;

                    _twitterURI = "https://twitter.com/" + SelectedProfile.TwitterID;
                    _githubURI = "https://github.com/" + SelectedProfile.GitHubID;
                    _linkedinUri = "https://www.linkedin.com/in/" + SelectedProfile.TwitterID;

                    TwitterEnabled = !string.IsNullOrWhiteSpace(SelectedProfile.TwitterID);
                    GitHubEnabled = !string.IsNullOrWhiteSpace(SelectedProfile.GitHubID);
                    LinkedinEnabled = !string.IsNullOrWhiteSpace(SelectedProfile.LinkedInId);

                    RaisePropertyChanged(OnSwipedUpdatePropertyList);

                    Skills.Clear();

                    foreach(var skill in SelectedProfile.Skills.OrderByDescending(s => s.Level).Take(5))
                    {
                        Skills.Add(skill);
                    }    
                }
                catch (Exception)
                {
                    // silently fail
                }
            }
        }

        private void NavigateForward()
        {
            var selectedIndex = Profiles.IndexOf(SelectedProfile);

            if (selectedIndex < _lastProfileIndex)
            {
                ScrollToRequested.Invoke(this, ++selectedIndex);
            }
            else
            {
                ScrollToRequested.Invoke(this, 0);
            }
        }

        private void NavigateBack()
        {
            var selectedIndex = Profiles.IndexOf(SelectedProfile);

            if (selectedIndex > 0)
            {
                ScrollToRequested.Invoke(this, --selectedIndex);
            }
            else
            {
                ScrollToRequested.Invoke(this, _lastProfileIndex);
            }
        }

        private async Task OpenTwitter()
        {
            await Launcher.OpenAsync(new Uri(_twitterURI));
        }

        private async Task OpenGithub()
        {
            await Launcher.OpenAsync(new Uri(_githubURI));
        }

        private async Task OpenLinkedin()
        {
            await Launcher.OpenAsync(new Uri(_linkedinUri));
        }
    }
}
