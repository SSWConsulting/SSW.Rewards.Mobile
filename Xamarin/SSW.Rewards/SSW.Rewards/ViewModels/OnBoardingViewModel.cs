using SSW.Rewards.Services;
using SSW.Rewards.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class OnBoardingViewModel : BaseViewModel
    {
        public ICommand DoActionCommand { get; set; }
		public ICommand Swiped { get; set; }
        public ObservableCollection<CarouselViewModel> Items { get; set; }
        public CarouselViewModel SelectedItem { get; set; }
        public string SubHeading { get; set; }
        public string Content { get; set; }
        public string ButtonText { get; set; }
        public Color BackgroundColour { get; set; }
        public string[] Properties { get; set; }

        public EventHandler<int> ScrollToRequested;

        private IUserService _userService { get; set; }

        public OnBoardingViewModel(IUserService userService)
        {
            _userService = userService;
            DoActionCommand = new Command(DoAction);
            Swiped = new Command(SetDetails);
            Properties = new string[] { nameof(SubHeading), nameof(Content), nameof(BackgroundColour), nameof(ButtonText)};
            Items = new ObservableCollection<CarouselViewModel>
            {
                new CarouselViewModel
                {
                    Content = "Talk to SSW people, attend their talks and scan their QR codes, and take the Tech Quiz to earn points.",
                    Image = "v2sophie",
                    SubHeading = "SSW rewards",
                    ButtonText = "GET STARTED",
                },
                new CarouselViewModel
                {
                    Content = "Talk to SSW people, attend their talks and scan their QR codes, and take the Tech Quiz to earn points.",
                    Image = "v2win",
                    SubHeading = "Earning points",
                    ButtonText = "NEXT",
                },
                new CarouselViewModel
                {
                    Content = "Get on the leaderboard for a chance to win a Google Hub Max.",
                    Image = "prize_hubmax",
                    SubHeading = "Google Nest Hub Max",
                    ButtonText = "NEXT",
                },
                new CarouselViewModel
                {
                    Content = "Earn enough points and you could claim a smart water bottle with touch activated content thermometer.",
                    Image = "v2cups",
                    SubHeading = "SSW Smart Keepcup",
                    ButtonText = "NEXT",
                },
                new CarouselViewModel
                {
                    Content = "Get on the leaderboard and earn a MI Wrist band. Just like a FitBit, except more functionality and a month's battery life!",
                    Image = "v2band",
                    SubHeading = "MI Band 4",
                    ButtonText = "NEXT",
                },
                new CarouselViewModel
                {
                    Content = "SSW Architects will help you successfully implement your project.",
                    Image = "v2consultation",
                    SubHeading = "Half Price Specification Review",
                    ButtonText = "DONE",
                }
            };

            SelectedItem = Items[0];

            SetDetails();
        }

        private async void DoAction()
        {
            try
            {
                // find next item
                var selectedIndex = Items.IndexOf(SelectedItem);

                var isFirstItem = selectedIndex == 0;

                var isLastItem = selectedIndex == Items.Count - 1;

                if (isLastItem)
                {
                    if (_userService.IsLoggedIn)
                    {
                        await Navigation.PopModalAsync();

                        /*AppShell shell = new AppShell();
                        //Application.Current.MainPage = shell;
                        Navigation.PushAsync(shell);*/

                    }
                    else
                    {
                        //Application.Current.MainPage = new LoginPage();
                        await Navigation.PushAsync(new LoginPage());
                    }
                }
                else
                {
                    ScrollToRequested.Invoke(this, ++selectedIndex);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            
        }

        private void SetDetails()
        {
            SubHeading = SelectedItem.SubHeading;
            Content = SelectedItem.Content;
            ButtonText = SelectedItem.ButtonText;
            RaisePropertyChanged(Properties);
        }
    }

    public class CarouselViewModel
    {
        public string SubHeading { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public string ButtonText { get; set; }
    }
}
