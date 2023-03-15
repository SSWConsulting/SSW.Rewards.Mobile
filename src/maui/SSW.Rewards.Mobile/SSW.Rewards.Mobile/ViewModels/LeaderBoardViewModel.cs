using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.IdentityModel.Tokens;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels
{
    public class LeaderBoardViewModel : BaseViewModel, IRecipient<PointsAwardedMessage>
    {
        private ILeaderService _leaderService;
        private IUserService _userService;
        private bool _loaded;
        private ObservableCollection<LeaderViewModel> searchResults = new ();
        private const string DismissIcon = "\ue4c3";
        private const string SearchIcon = "\uea7c";
        
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
            WeakReferenceMessenger.Default.Register(this);
        }
        
        public ICommand LeaderTapped => new Command<LeaderViewModel>(async (x) => await HandleLeaderTapped(x));
        public ICommand OnRefreshCommand { get; set; }
        public ICommand ScrollToTopCommand => new Command(ScrollToFirstCard);
        public ICommand ScrollToEndCommand => new Command(ScrollToLastCard);
        public ICommand ScrollToMeCommand => new Command(ScrollToMyCard);
        public ICommand RefreshCommand => new Command(async () => await RefreshLeaderboard());
        public ICommand ClearSearchCommand => new Command(ClearSearch);
        public ICommand GoToMyProfileCommand => new Command(async () => await Shell.Current.GoToAsync("//main"));
        public ICommand SearchTextChangedCommand => new Command(SearchTextChanged);
        public ICommand FilterByPeriodCommand => new Command(FilterByPeriod);

        public ObservableCollection<LeaderViewModel> Leaders { get; set; }
        public bool IsRunning { get; set; }
        public bool IsRefreshing { get; set; }
        public string ProfilePic { get; set; }
        public Action<int> ScrollTo { get; set; }
        public PeriodFilter CurrentPeriod { get; set; }
        public int TotalLeaders { get; set; }
        public int MyRank { get; set; }
        public int MyPoints { get; set; }
        public int MyBalance { get; set; }
        public string SearchBarIcon { get; set; } = SearchIcon;
        public string SearchBarText { get; set; }
        

        private void ClearSearch()
        {
            SearchBarText = string.Empty;
            SearchBarIcon = SearchIcon;
            RaisePropertyChanged(nameof(SearchBarText), nameof(SearchBarIcon));
        }
        
        public ObservableCollection<LeaderViewModel> SearchResults
        {
            get => searchResults;
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

                FilterAndSortLeaders(Leaders, PeriodFilter.Month);

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

                Leaders.Add(vm);
            }

            TotalLeaders = summaries.Count();

            OnPropertyChanged(nameof(TotalLeaders));

            ScrollTo?.Invoke(0);
        }

        private void SearchTextChanged()
        {
            bool keepRanks = false;
            if (SearchBarText.IsNullOrEmpty())
            {
                SearchBarText = string.Empty;
                SearchBarIcon = SearchIcon; SearchResults.Clear();
            }
            else
            {
                SearchBarIcon = DismissIcon;
                keepRanks = true;
            }
            OnPropertyChanged(nameof(SearchBarIcon));
            var filtered = Leaders.Where(l => l.Name.ToLower().Contains(SearchBarText.ToLower()));
            FilterAndSortLeaders(filtered, CurrentPeriod, keepRanks);
        }

        private void FilterByPeriod()
        {
            // If Search is empty when we switch tabs, ClearSearchCommand won't update the list because
            // PropertyChanged won't trigger SearchTextChanged, so we need to call it here.
            var needToSort = SearchBarText.IsNullOrEmpty();
            ClearSearchCommand.Execute(null);
            if (needToSort)
            {
                FilterAndSortLeaders(Leaders, CurrentPeriod);
            }
        }

        public void FilterAndSortLeaders(IEnumerable<LeaderViewModel> list, PeriodFilter period, bool keepRank = false)
        {
            switch (period)
            {
                case PeriodFilter.Month:
                    FilterAndSortByThisMonth(list, keepRank);
                    break;
                case PeriodFilter.Year:
                    FilterAndSortByThisYear(list, keepRank);
                    break;
                case PeriodFilter.AllTime:
                default:
                    FilterAndSortByAllTime(list, keepRank);
                    break;
            }
        }

        private void FilterAndSortByThisMonth(IEnumerable<LeaderViewModel> list, bool keepRank)
        {
            SearchResults.Clear();
            var leaders = list.OrderByDescending(l => l.PointsThisMonth);
            int rank = 1;
            foreach (var leader in leaders)
            {
                if (!keepRank)
                {
                    leader.Rank = rank;
                    rank++;
                }

                leader.DisplayPoints = leader.PointsThisMonth;
                SearchResults.Add(leader);
            }

            UpdateMyRankIfRequired(leaders.FirstOrDefault(l => l.IsMe == true));
        }

        private void FilterAndSortByThisYear(IEnumerable<LeaderViewModel> list, bool keepRank)
        {
            SearchResults.Clear();
            var leaders = list.OrderByDescending(l => l.PointsThisYear);
            int rank = 1;
            foreach (var leader in leaders)
            {
                if (!keepRank)
                {
                    leader.Rank = rank;
                    rank++;
                }

                leader.DisplayPoints = leader.PointsThisYear;
                SearchResults.Add(leader);
            }

            UpdateMyRankIfRequired(leaders.FirstOrDefault(l => l.IsMe == true));
        }

        private void FilterAndSortByAllTime(IEnumerable<LeaderViewModel> list, bool keepRank)
        {
            SearchResults.Clear();
            var leaders = list.OrderByDescending(l => l.TotalPoints);
            int rank = 1;
            foreach (var leader in leaders)
            {
                if (!keepRank)
                {
                    leader.Rank = rank;
                    rank++;
                }

                leader.DisplayPoints = leader.TotalPoints;
                SearchResults.Add(leader);
            }

            UpdateMyRankIfRequired(leaders.FirstOrDefault(l => l.IsMe == true));
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

            FilterAndSortLeaders(Leaders, CurrentPeriod);
            
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
                await Shell.Current.Navigation.PushModalAsync<ProfilePage>(leader);
        }

        public async void Receive(PointsAwardedMessage message)
        {
            await Refresh();
        }

        private void UpdateMyRankIfRequired(LeaderViewModel mySummary)
        {
            if (mySummary is not null)
            {
                MyRank = mySummary.Rank;
                OnPropertyChanged(nameof(MyRank));
            }
        }
    }
}
