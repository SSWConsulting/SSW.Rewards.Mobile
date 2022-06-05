using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SSW.Rewards.Services;
using System.Linq;
using Xamarin.Forms;
using System.Threading.Tasks;
using SSW.Rewards.Pages;

namespace SSW.Rewards.ViewModels
{
    public class LeaderBoardViewModel : BaseViewModel
    {
        public bool IsRunning { get; set; }
        public bool IsRefreshing { get; set; }

        public string ProfilePic { get; set; }

        private ILeaderService _leaderService;
        
        private IUserService _userService;
        
        public ICommand LeaderTapped => new Command<LeaderSummaryViewModel>(async (x) => await HandleLeaderTapped(x));

        public ICommand OnRefreshCommand { get; set; }

        public ICommand ScrollToTopCommand => new Command(() => MessagingCenter.Send("ScrollToTop", "ScrollToTop"));

        public ObservableCollection<LeaderSummaryViewModel> Leaders { get; set; }

        public Action<LeaderSummaryViewModel> ScrollToMe { get; set; }

        public Action<LeaderSummaryViewModel> ScrollToTop { get; set; }

        private ObservableCollection<LeaderSummaryViewModel> searchResults = new ObservableCollection<LeaderSummaryViewModel>();

        public int TotalLeaders { get; set; }

        public int MyRank { get; set; }

        public int MyPoints { get; set; }

        public int MyBalance { get; set; }

        public LeaderBoardViewModel(ILeaderService leaderService, IUserService userService)
        {
            Title = "Leaderboard";
            OnRefreshCommand = new Command(Refresh);
            _leaderService = leaderService;
            _userService = userService;
            ProfilePic = _userService.MyProfilePic;

            MyPoints = _userService.MyPoints;

            MyBalance = _userService.MyBalance;

            Leaders = new ObservableCollection<LeaderSummaryViewModel>();
            MessagingCenter.Subscribe<object>(this, "NewAchievement", (obj) => { Refresh(); });
            MessagingCenter.Subscribe<string>(this, "ProfilePicChanged", (obj) => { Refresh(); });
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

        public async Task Initialise()
        {
            IsRunning = true;
            RaisePropertyChanged("IsRunning");
            IEnumerable<Models.LeaderSummary> summaries = await _leaderService.GetLeadersAsync(false);
            int myId = _userService.MyUserId;

            foreach (Models.LeaderSummary summary in summaries)
            {
                LeaderSummaryViewModel vm = new LeaderSummaryViewModel(summary);
                vm.IsMe = (summary.id == myId);

                Leaders.Add(vm);
            }

            IsRunning = false;
            SearchResults = Leaders;

            var mysummary = Leaders.FirstOrDefault(l => l.IsMe == true);

            TotalLeaders = summaries.Count();

            MyRank = mysummary.Rank;

            RaisePropertyChanged(nameof(IsRunning), nameof(searchResults), nameof(MyRank), nameof(TotalLeaders));

            ScrollToMe?.Invoke(mysummary);
            var firstLeader = Leaders.FirstOrDefault();
            ScrollToTop?.Invoke(firstLeader);
        }


        public async void Refresh()
        {
            // = true;
            //RaisePropertyChanged("IsRunning");
            var summaries = await _leaderService.GetLeadersAsync(false);
            int myId = _userService.MyUserId;

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

        private async Task HandleLeaderTapped(LeaderSummaryViewModel leader)
        {
            if (leader.IsMe)
                await Shell.Current.GoToAsync("main");
            else
                await Shell.Current.Navigation.PushAsync(new ProfilePage(leader));
        }
    }
}
