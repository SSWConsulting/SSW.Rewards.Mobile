using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SSW.Rewards.Mobile.ViewModels
{
    public partial class OnBoardingViewModel : BaseViewModel
    {
        public ICommand DoActionCommand { get; set; }
		public ICommand Swiped { get; set; }
        public ICommand Skip { get; set; }
        public ObservableCollection<CarouselViewModel> Items { get; set; }
        public CarouselViewModel SelectedItem { get; set; }

        [ObservableProperty]
        private string _subHeading;
        
        [ObservableProperty]
        private string _content;
        
        [ObservableProperty]
        private string _buttonText;
        
        [ObservableProperty]
        private Color _backgroundColour;
        
        [ObservableProperty]
        private int _points;
        
        [ObservableProperty]
        private bool _hasPoints;

        public EventHandler<int> ScrollToRequested;

        public OnBoardingViewModel()
        {
            DoActionCommand = new Command(DoAction);
            Swiped = new Command(HandleSwiped);
            Skip = new Command(async () => await SkipOnboarding());
            Items = new ObservableCollection<CarouselViewModel>
            {
                new CarouselViewModel
                {
                    Content = "Talk to SSW people, attend their talks and scan their QR codes, and complete other fun achievements to earn points.",
                    //Animation = "Sophie.json", TODO: MAUI, timing issue in SKLottieView https://github.com/mono/SkiaSharp.Extended/issues/142
                    SubHeading = "Welcome!",
                    ButtonText = "GET STARTED",
                    //IsAnimation = true
                    Image = "sophie_hello",
                    IsAnimation = false,
                },
                new CarouselViewModel
                {
                    Content = "Exchange your points at SSW Events or at SSW booths at developer conferences for awesome rewards.",
                    Image = "test_passed",
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
                new CarouselViewModel
                {
                    Content = "Earn enough points and you could claim a cool SSW Keepcup.",
                    Image = "v2cups",
                    SubHeading = "SSW Keepcup",
                    HasPoints = true,
                    Points = 2000,
                    ButtonText = "NEXT",
                    IsAnimation = false
                },
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
                    Image = "v2consultation.JPG",
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

        private void HandleSwiped()
        {
            // TECH DEBT: We don't know why tf this happens
            if (SelectedItem is null)
                return;
            SetDetails();
        }

        private void SetDetails()
        {
            SubHeading = SelectedItem.SubHeading;
            Content = SelectedItem.Content;
            ButtonText = SelectedItem.ButtonText;
            HasPoints = SelectedItem.HasPoints;
            Points = SelectedItem.Points;
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
