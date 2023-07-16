using System.Collections.ObjectModel;
using System.Linq;
using SSW.Rewards.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LeaderboardPage
    {
        private readonly LeaderBoardViewModel _viewModel;

        public LeaderboardPage()
        {
            _viewModel = Resolver.Resolve<LeaderBoardViewModel>();
            _viewModel.Navigation = Navigation;
            BindingContext = _viewModel;
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.Initialise();
            _viewModel.ScrollTo += ScrollTo;
        }

        private void FilterChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                var radio = (RadioButton)sender;

                switch (radio.Content as string)
                {
                    case "This Month":
                        _viewModel.SortLeaders("month");
                        break;
                    case "This Year":
                        _viewModel.SortLeaders("year");
                        break;
                    default:
                        _viewModel.SortLeaders("all");
                        break;
                }
            }
        }


        private void ScrollTo(int i)
        {
            LeadersCollection.ScrollTo(i, position: ScrollToPosition.Center);
        }

        private void CollectionView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            if (e.FirstVisibleItemIndex > 1)
            {
                // show the scrolling controls
                ScrollButtons.IsVisible = true;
            }
            else
            {
                // hide the scrolling controls
                ScrollButtons.IsVisible = false;
            }
        }

        private void Search_OnSearchTextChanged(object sender, string e)
        {
            // TODO: check time filter, or switch to all time when searching
            var searchText = e.ToLower();
            var filtered = _viewModel.Leaders.Where(l => l.Name.ToLower().Contains(searchText));
            _viewModel.SearchResults = new ObservableCollection<LeaderViewModel>(filtered);
        }
    }
}