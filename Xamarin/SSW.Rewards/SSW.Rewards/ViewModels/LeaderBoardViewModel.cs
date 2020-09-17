using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SSW.Rewards.Services;
using SSW.Rewards.Views;
using System.Linq;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace SSW.Rewards.ViewModels
{
    public class LeaderBoardViewModel : BaseViewModel
    {
        public bool IsRunning { get; set; }
        public bool IsRefreshing { get; set; }

        private ILeaderService _leaderService;
        private IUserService _userService;
        public ICommand LeaderTapped
        {
            get
            {
                return new Command<LeaderSummaryViewModel>((x) => HandleLeaderTapped(x));
            }
        }
        public ICommand OnRefreshCommand { get; set; }

        public ObservableCollection<LeaderSummaryViewModel> Leaders { get; set; }

        public Action<LeaderSummaryViewModel> ScrollToMe { get; set; }

        private ObservableCollection<LeaderSummaryViewModel> searchResults = new ObservableCollection<LeaderSummaryViewModel>();


        public LeaderBoardViewModel(ILeaderService leaderService, IUserService userService)
        {
            Title = "Leaderboard";
            OnRefreshCommand = new Command(Refresh);
            _leaderService = leaderService;
            _userService = userService;
            Leaders = new ObservableCollection<LeaderSummaryViewModel>();
            MessagingCenter.Subscribe<object>(this, "NewAchievement", (obj) => { Refresh(); });
            MessagingCenter.Subscribe<string>(this, "ProfilePicChanged", (obj) => { Refresh(); });
            _ = Initialise();
        }


        public ICommand SearchTextChanged => new Command<string>((string query) =>
        {
            if (query != null || query != String.Empty)
            {
                var filtered = Leaders.Where(l => l.Name.ToLower().Contains(query.ToLower()));
                SearchResults = new ObservableCollection<LeaderSummaryViewModel>(filtered);

                return;
            }
            SearchResults = Leaders;

        });

        public ObservableCollection<LeaderSummaryViewModel> SearchResults
        {
            get
            {
                return searchResults;
            }
            set
            {
                searchResults = value;
                RaisePropertyChanged("SearchResults");

            }
        }

        private async Task Initialise()
        {
            IsRunning = true;
            RaisePropertyChanged("IsRunning");
            IEnumerable<Models.LeaderSummary> summaries = await _leaderService.GetLeadersAsync(false);
            int myId = await _userService.GetMyUserIdAsync();

            foreach (Models.LeaderSummary summary in summaries)
            {
                LeaderSummaryViewModel vm = new LeaderSummaryViewModel(summary);
                vm.IsMe = (summary.id == myId);

                Leaders.Add(vm);
            }

            IsRunning = false;
            RaisePropertyChanged("IsRunning");

            SearchResults = Leaders;
            RaisePropertyChanged("SearchResults");

            var mysummary = Leaders.FirstOrDefault(l => l.IsMe == true);
            ScrollToMe?.Invoke(mysummary);
        }


        public async void Refresh()
        {
            // = true;
            //RaisePropertyChanged("IsRunning");
            var summaries = await _leaderService.GetLeadersAsync(false);
            int myId = await _userService.GetMyUserIdAsync();

            Leaders.Clear();
            
            foreach (var summary in summaries)
            {
                LeaderSummaryViewModel vm = new LeaderSummaryViewModel(summary);
                vm.IsMe = (summary.id == myId);

                Leaders.Add(vm);
            }
            SearchResults = Leaders;
            
            IsRefreshing = false;
            RaisePropertyChanged("IsRefreshing");
        }

        private void HandleLeaderTapped(LeaderSummaryViewModel leader)
        {
            if (leader.IsMe)
                Shell.Current.Navigation.PushAsync(new Profile());
            else
                Shell.Current.Navigation.PushAsync(new Profile(leader));
        }
    }
}
