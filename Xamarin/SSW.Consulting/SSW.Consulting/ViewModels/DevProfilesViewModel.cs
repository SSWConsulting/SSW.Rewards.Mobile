using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SSW.Consulting.Models;
using SSW.Consulting.Services;
using Xamarin.Forms;

namespace SSW.Consulting.ViewModels
{
    public class DevProfilesViewModel : BaseViewModel
    {
        private IDevService _devService;
        private IContacts _contacts;

        public ICommand OnCardSwiped { get; set; }
        public ICommand HandleProfileTapped { get; set; }
        public ICommand HandleScrollTapped { get; set; }
        public ICommand OnTwitterTapped { get; set; }
        public ICommand OnContactTapped { get; set; }

        private bool _profileExpanded { get; set; }
        public bool IsRunning { get; set; }

        public ObservableCollection<DevProfile> Profiles { get; set; }

        public int PositionSelected { get; set; }

        public string DevFirstName { get; set; }
        public string DevTitle { get; set; }
        public string DevBio { get; set; }

        private string _twitterURI { get; set; }
        private string _devEmail { get; set; }
        private string _devPhone { get; set; }

        //public LayoutBo MyProperty { get; set; }

        public Rectangle OverlayLayoutBounds { get; set; }

        private string _devBio { get; set; }

        public string[] OnSwipedUpdatePropertyList { get; set; }

        public DevProfilesViewModel(IDevService devService)
        {
            IsRunning = true;
            OnPropertyChanged("IsRunning");
            _devService = devService;
            //_contacts = contacts;
            OnCardSwiped = new Command(SetDevDetails);
            HandleProfileTapped = new Command(ExpandCollapseProfile);
            HandleScrollTapped = new Command(ExpandCollapseProfile);
            OnTwitterTapped = new Command(OpenTwitter);
            OnContactTapped = new Command(SaveSelectedContact);

            Profiles = new ObservableCollection<DevProfile>();

            OverlayLayoutBounds = new Rectangle(1, 1, 1, 0.2);
            _profileExpanded = false;
            OnSwipedUpdatePropertyList = new string[] { "Title", "DevFirstName", "DevTitle", "DevBio" };
            Initialise();
        }

        private async void Initialise()
        {
            var profiles = await _devService.GetProfilesAsync();
            foreach(var profile in profiles)
            {
                Profiles.Add(profile);
            }
            OnPropertyChanged("Profiles");
            SetDevDetails();
            IsRunning = false;
            OnPropertyChanged("IsRunning");
        }

        public void SetDevDetails()
        {
            int profileIndex = PositionSelected;
            Title = $"{Profiles[profileIndex].FirstName} {Profiles[profileIndex].LastName}";
            DevFirstName = Profiles[profileIndex].FirstName;
            DevTitle = Profiles[profileIndex].Title;
            DevBio = Profiles[profileIndex].Bio;
            _twitterURI = "https://twitter.com/" + Profiles[profileIndex].TwitterID;
            _devEmail = Profiles[profileIndex].Email;
            _devPhone = Profiles[profileIndex].Phone;
            RaisePropertyChanged(OnSwipedUpdatePropertyList);
            int devId = Profiles[profileIndex].id;
            MessagingCenter.Send<object, int>(this, "DevChanged", devId);

            //App.Current.MainPage.DisplayAlert("Twitter", "ID: " + _twitterURI + Environment.NewLine + "Index: " + profileIndex, "OK");
        }

        private async void ExpandCollapseProfile()
        {
            if(_profileExpanded)
            {
                //collapse the profile
                DevBio = string.Empty;
                MessagingCenter.Send<object>(this, "SlideDown");
            }
            else
            {
                //expand the profile
                DevBio = _devBio;
                MessagingCenter.Send<object>(this, "SlideUp");
            }

            _profileExpanded = !_profileExpanded;
        }

        private async void OpenTwitter()
        {
            //Uri twitterURI = new Uri("https://twitter.com/" + devTwitter);

            Device.OpenUri(new Uri(_twitterURI));
        }

        private async void SaveSelectedContact()
        {
            //_contacts.SaveContact(Title, _devPhone, _devEmail);
        }
    }
}
