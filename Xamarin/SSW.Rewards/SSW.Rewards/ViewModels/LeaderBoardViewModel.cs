using SSW.Rewards.Pages;
using SSW.Rewards.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class LeaderBoardViewModel : BaseViewModel
    {
        public bool IsRunning { get; set; }
        public bool IsRefreshing { get; set; }

        public string ProfilePic { get; set; }

        private ILeaderService _leaderService;
        
        private IUserService _userService;
        
        public ICommand LeaderTapped => new Command<LeaderViewModel>(async (x) => await HandleLeaderTapped(x));

        public ICommand OnRefreshCommand { get; set; }

        public ICommand ScrollToTopCommand => new Command(() => MessagingCenter.Send("ScrollToTop", "ScrollToTop"));

        public ICommand RefreshCommand => new Command(async () => await RefreshLeaderboard());

        public ObservableCollection<LeaderViewModel> Leaders { get; set; }

        public Action<LeaderViewModel> ScrollToMe { get; set; }

        public Action<LeaderViewModel> ScrollToTop { get; set; }

        private ObservableCollection<LeaderViewModel> searchResults = new ObservableCollection<LeaderViewModel>();

        public int TotalLeaders { get; set; }

        public int MyRank { get; set; }

        public int MyPoints { get; set; }

        public int MyBalance { get; set; }

        bool _loaded = false;

        private string _sortFilter;

        public LeaderBoardViewModel(ILeaderService leaderService, IUserService userService)
        {
            Title = "Leaderboard";
            OnRefreshCommand = new Command(Refresh);
            _leaderService = leaderService;
            _userService = userService;

            ProfilePic = _userService.MyProfilePic;

            MyPoints = _userService.MyPoints;

            MyBalance = _userService.MyBalance;

            Leaders = new ObservableCollection<LeaderViewModel>();
            MessagingCenter.Subscribe<object>(this, "NewAchievement", (obj) => { Refresh(); });
            MessagingCenter.Subscribe<string>(this, "ProfilePicChanged", (obj) => { Refresh(); });

            _sortFilter = "all";
        }


        public ICommand SearchTextChanged => new Command<string>((string query) =>
        {
            // TODO: check time filter, or switch to all time when searching
            if (query != null || query != String.Empty)
            {
                var filtered = Leaders.Where(l => l.Name.ToLower().Contains(query.ToLower()));
                SearchResults = new ObservableCollection<LeaderViewModel>(filtered);
                return;
            }
        });

        public ObservableCollection<LeaderViewModel> SearchResults
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
            if (!_loaded)
            {
                IsRunning = true;
                RaisePropertyChanged(nameof(IsRunning));

                await LoadLeaderboard();
                _loaded = true;

                SortByThisMonth();

                IsRunning = false;
                RaisePropertyChanged(nameof(IsRunning));
            }
        }

        private async Task RefreshLeaderboard()
        {
            await LoadLeaderboard();

            IsRefreshing = false;
            OnPropertyChanged(nameof(IsRefreshing));
        }

        private async Task LoadLeaderboard()
        {
            var summaries = await _leaderService.GetLeadersAsync(false);

            int myId = _userService.MyUserId;

            Leaders.Clear();

            foreach (var summary in summaries)
            {
                var isMe = myId == summary.UserId;
                var vm = new LeaderViewModel(summary, isMe);

                Console.WriteLine($"[LeaderboardViewModel] ${summary.Name} is me: {isMe}");

                Leaders.Add(vm);
            }

            TotalLeaders = summaries.Count();

            OnPropertyChanged(nameof(TotalLeaders));

            var firstLeader = Leaders.FirstOrDefault();
            ScrollToTop?.Invoke(firstLeader);
        }

        public void SortLeaders(string filter)
        {
            _sortFilter = filter;

            switch(_sortFilter)
            {
                case "month":
                    SortByThisMonth();
                    break;
                case "year":
                    SortByThisYear();
                    break;
                case "all":
                default:
                    SortByAlltime();
                    break;
            }
        }

        private void SortByThisMonth()
        {
            SearchResults.Clear();

            var leaders = Leaders.OrderByDescending(l => l.PointsThisMonth);

            int rank = 1;

            foreach(var leader in leaders)
            {
                leader.Rank = rank;

                leader.DisplayPoints = leader.PointsThisMonth;

                SearchResults.Add(leader);

                rank++;
            }

            var mysummary = leaders.FirstOrDefault(l => l.IsMe == true);

            MyRank = mysummary.Rank;

            OnPropertyChanged(nameof(MyRank));
        }

        private void SortByThisYear()
        {
            SearchResults.Clear();

            var leaders = Leaders.OrderByDescending(l => l.PointsThisYear);

            int rank = 1;

            foreach (var leader in leaders)
            {
                leader.Rank = rank;

                leader.DisplayPoints = leader.PointsThisYear;

                SearchResults.Add(leader);

                rank++;
            }

            var mysummary = leaders.FirstOrDefault(l => l.IsMe == true);

            MyRank = mysummary.Rank;

            OnPropertyChanged(nameof(MyRank));
        }

        private void SortByAlltime()
        {
            SearchResults.Clear();

            var leaders = Leaders.OrderByDescending(l => l.TotalPoints);

            int rank = 1;

            foreach (var leader in leaders)
            {
                leader.Rank = rank;

                leader.DisplayPoints = leader.TotalPoints;

                SearchResults.Add(leader);

                rank++;
            }

            var mysummary = leaders.FirstOrDefault(l => l.IsMe == true);

            MyRank = mysummary.Rank;

            OnPropertyChanged(nameof(MyRank));
        }

        public async void Refresh()
        {
            var summaries = await _leaderService.GetLeadersAsync(false);
            int myId = _userService.MyUserId;

            Leaders.Clear();
            
            foreach (var summary in summaries)
            {
                var isMe = myId == summary.UserId;
                var vm = new LeaderViewModel(summary, isMe);

                Leaders.Add(vm);
            }

            SortLeaders(_sortFilter);
            
            IsRefreshing = false;
            
            RaisePropertyChanged("IsRefreshing");
        }

        private async Task HandleLeaderTapped(LeaderViewModel leader)
        {
            if (leader.IsMe)
                await Shell.Current.GoToAsync("main");
            else
                await Shell.Current.Navigation.PushAsync(new ProfilePage(leader));
        }
    }
}
