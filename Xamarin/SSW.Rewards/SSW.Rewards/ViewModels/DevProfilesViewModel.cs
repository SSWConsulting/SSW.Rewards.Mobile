using System;
using System.Collections.ObjectModel;
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

        public ICommand OnCardSwiped => new Command(SetDevDetails);

        public ICommand OnTwitterTapped => new Command(async () => await OpenTwitter());
        public ICommand OnGithubTapped => new Command(async () => await OpenGithub());
        public ICommand OnLinkedinTapped => new Command(async () => await OpenLinkedin());

        public bool IsRunning { get; set; }

        public bool TwitterEnabled { get; set; }
        public bool GitHubEnabled { get; set; }
        public bool LinkedinEnabled { get; set; }

        public ObservableCollection<DevProfile> Profiles { get; set; } = new ObservableCollection<DevProfile>();

        public DevProfile SelectedProfile { get; set; }

        public string DevName { get; set; }
        public string DevTitle { get; set; }
        public string DevBio { get; set; }

        private string _twitterURI;
        private string _githubURI;
        private string _linkedinUri;

        private bool _initialised = false;

        public string[] OnSwipedUpdatePropertyList { get; set; }

        public DevProfilesViewModel(IDevService devService)
        {
            IsRunning = true;
            TwitterEnabled = true;
            _devService = devService;

            OnSwipedUpdatePropertyList = new string[] { nameof(Title), nameof(DevName), nameof(DevTitle), nameof(DevBio), nameof(TwitterEnabled) };
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

            SelectedProfile = Profiles[0];

            SetDevDetails();

            RaisePropertyChanged(nameof(IsRunning));
        }

        public void SetDevDetails()
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

                    Console.WriteLine($"[Dev profile viewmodel] Selected picture: {SelectedProfile.Picture}");
                }
                catch (Exception)
                {
                    // silently fail
                }
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
