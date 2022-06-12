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

        public ICommand ScrollToTopCommand => new Command(ScrollToFirstCard);

        public ICommand ScrollToEndCommand => new Command(ScrollToLastCard);

        public ICommand ScrollToMeCommand => new Command(ScrollToMyCard);

        public ICommand RefreshCommand => new Command(async () => await RefreshLeaderboard());

        public ICommand CancelSearchCommand => new Command(CancelSearch);

        public ObservableCollection<LeaderViewModel> Leaders { get; set; }

        public Action<int> ScrollTo { get; set; }


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
            OnRefreshCommand = new Command(async () => await Refresh());
            _leaderService = leaderService;
            _userService = userService;

            ProfilePic = _userService.MyProfilePic;

            MyPoints = _userService.MyPoints;

            MyBalance = _userService.MyBalance;

            Leaders = new ObservableCollection<LeaderViewModel>();
            MessagingCenter.Subscribe<object>(this, ScannerService.PointsAwardedMessage, async (obj) => await Refresh());

            _sortFilter = "all";
        }

        public ICommand SearchTextChanged => new Command(() =>
        {
            // TODO: check time filter, or switch to all time when searching
            if (SearchBarText != null || SearchBarText != String.Empty)
            {
                var filtered = Leaders.Where(l => l.Name.ToLower().Contains(SearchBarText.ToLower()));
                SearchResults = new ObservableCollection<LeaderViewModel>(filtered);
                SearchBarIcon = DismissIcon;
                OnPropertyChanged(nameof(SearchBarIcon));
                return;
            }

            SearchBarIcon = SearchIcon;
            OnPropertyChanged(nameof(SearchBarIcon));
        });

        private void CancelSearch()
        {
            SearchBarText = string.Empty;
            SearchBarIcon = SearchIcon;
            RaisePropertyChanged(nameof(SearchBarText), nameof(SearchBarIcon));
        }

        private const string DismissIcon = "\ue4c3";
        private const string SearchIcon = "\uea7c";

        public string SearchBarIcon { get; set; } = SearchIcon;

        public string SearchBarText { get; set; }

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

            ScrollTo?.Invoke(0);
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

            foreach (var leader in leaders)
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

        public async Task Refresh()
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

        private void ScrollToMyCard()
        {
            var myCard = SearchResults.FirstOrDefault(l => l.IsMe);
            var myIndex = SearchResults.IndexOf(myCard);

            ScrollTo.Invoke(myIndex);
        }

        private void ScrollToFirstCard()
        {
            ScrollTo.Invoke(0);
        }

        private void ScrollToLastCard()
        {
            ScrollTo.Invoke(SearchResults.Count() - 1);
        }

        private async Task HandleLeaderTapped(LeaderViewModel leader)
        {
            if (leader.IsMe)
                await Shell.Current.GoToAsync("//main");
            else
                await Shell.Current.Navigation.PushModalAsync(new ProfilePage(leader));
        }
    }
}
