using SSW.Rewards.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class OnBoardingViewModel : BaseViewModel
    {
        public ICommand DoActionCommand { get; set; }
		public ICommand Swiped { get; set; }
        public ICommand Skip { get; set; }
        public ObservableCollection<CarouselViewModel> Items { get; set; }
        public CarouselViewModel SelectedItem { get; set; }
        public string SubHeading { get; set; }
        public string Content { get; set; }
        public string ButtonText { get; set; }
        public Color BackgroundColour { get; set; }
        public int Points { get; set; }
        public bool HasPoints { get; set; }
        public string[] Properties { get; set; }

        public EventHandler<int> ScrollToRequested;

        private IUserService _userService { get; set; }

        public OnBoardingViewModel(IUserService userService)
        {
            _userService = userService;
            DoActionCommand = new Command(DoAction);
            Swiped = new Command(SetDetails);
            Skip = new Command(async () => await SkipOnboarding());
            Properties = new string[] { nameof(SubHeading), nameof(Content), nameof(BackgroundColour), nameof(ButtonText), nameof(Points), nameof(HasPoints)};
            Items = new ObservableCollection<CarouselViewModel>
            {
                new CarouselViewModel
                {
                    Content = "Talk to SSW people, attend their talks and scan their QR codes, and complete other fun achievements to earn points.",
                    Animation = "Sophie.json",
                    SubHeading = "Welcome!",
                    ButtonText = "GET STARTED",
                    IsAnimation = true
                },
                new CarouselViewModel
                {
                    Content = "Exchange your points at SSW Events or at SSW booths at developer conferences for awesome rewards.",
                    Image = "v2win",
                    SubHeading = "Claim Rewards",
                    ButtonText = "NEXT",
                    IsAnimation = false
                },
                new CarouselViewModel
                {
                    Content = "Get on the leaderboard for a chance to win a Google Hub Max.",
                    Image = "prize_hubmax",
                    SubHeading = "Google Nest Hub Max",
                    ButtonText = "NEXT",
                    IsAnimation = false
                },
                // TODO: uncomment when we get keepcups available again (as per conversation with camilla@ssw.com.au)
                //new CarouselViewModel
                //{
                //    Content = "Earn enough points and you could claim a smart water bottle with touch activated content thermometer.",
                //    Image = "v2cups",
                //    SubHeading = "SSW Smart Keepcup",
                //    HasPoints = true,
                //    Points = 2000,
                //    ButtonText = "NEXT",
                //    IsAnimation = false
                //},
                new CarouselViewModel
                {
                    Content = "Get on the leaderboard and earn a Mi Wrist band. Just like a FitBit, except more functionality and a month's battery life!",
                    Image = "v2band",
                    SubHeading = "Mi Band 6",
                    HasPoints = true,
                    Points = 6000,
                    ButtonText = "NEXT",
                    IsAnimation = false
                },
                new CarouselViewModel
                {
                    Content = "SSW Architects will help you successfully implement your project.",
                    Image = "v2consultation",
                    SubHeading = "Half Price Specification Review",
                    HasPoints = true,
                    Points = 3000,
                    ButtonText = "DONE",
                    IsAnimation = false
                }
            };

            SelectedItem = Items[0];

            SetDetails();
        }

        private async void DoAction()
        {
            // find next item
            var selectedIndex = Items.IndexOf(SelectedItem);

            var isFirstItem = selectedIndex == 0;

            var isLastItem = selectedIndex == Items.Count - 1;

            if (isLastItem)
            {
                await SkipOnboarding();
            }
            else
            {
                ScrollToRequested.Invoke(this, ++selectedIndex);
            }
        }

        private async Task SkipOnboarding()
        {
            await Navigation.PopModalAsync();
        }

        private void SetDetails()
        {
            SubHeading = SelectedItem.SubHeading;
            Content = SelectedItem.Content;
            ButtonText = SelectedItem.ButtonText;
            HasPoints = SelectedItem.HasPoints;
            Points = SelectedItem.Points;
            RaisePropertyChanged(Properties);
        }
    }

    public class CarouselViewModel
    {
        public string SubHeading { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public string ButtonText { get; set; }
        public bool HasPoints { get; set; } = false;
        public int Points { get; set; }
        public bool IsAnimation { get; set; }
        public string Animation { get; set; }
    }
}
