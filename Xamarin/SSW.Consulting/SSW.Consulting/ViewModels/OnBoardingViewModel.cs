using Microsoft.AppCenter.Auth;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using SSW.Consulting.Views;
using SSW.Consulting.Services;
using System.Threading.Tasks;

namespace SSW.Consulting.ViewModels
{
    public class OnBoardingViewModel : BaseViewModel
    {
        public ICommand GetStartedTapped { get; set; }
		public ICommand Swiped { get; set; }
        public ObservableCollection<CarouselViewModel> Items { get; set; }
        public int SelectedItem { get; set; }
        public string MainHeading { get; set; }
        public string SubHeading { get; set; }
        public string Content { get; set; }
        public string LinkText { get; set; }
        public Color BackgroundColour { get; set; }
        public Color TextColour { get; set; }
        public TextAlignment TextAlignment { get; set; }
        public string[] Properties { get; set; }

        private IUserService _userService { get; set; }

        public OnBoardingViewModel(IUserService userService)
        {
            _userService = userService;
            GetStartedTapped = new Command(GetStarted);
            Swiped = new Command(SetDetails);
            Properties = new string[] { "MainHeading", "SubHeading", "Content", "BackgroundColour", "TextColour", "LinkText", "TextAlignment" };
            Items = new ObservableCollection<CarouselViewModel>
            {
                new CarouselViewModel
                {
                    backgroundColour = Color.White /*(Color) Application.Current.Resources["SSWRed"]*/,
                    Content = "Talk to SSW people, attend their talks and scan their QR codes, and take the Tech Quiz to earn points.",
                    Image = "onboarding",
                    MainHeading = "Welcome",
                    SubHeading = "Earn rewards...",
                    TextColour = Color.Black,
                    LinkText = "SKIP INTRO",
                    textAlignment = TextAlignment.Start
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "Earn enough points and you could claim a smart water bottle with touch activated content thermometer.",
                    Image = "prize_waterbottles",
                    MainHeading = "Earn Rewards",
                    SubHeading = "...like a Smart Water Bottle...",
                    TextColour = Color.Black,
                    LinkText = "SKIP INTRO",
                    textAlignment = TextAlignment.Start
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "Get on the top of the leaderboard and win a Google Hub Max.",
                    Image = "prize_hubmax",
                    MainHeading = "Earn Rewards",
                    SubHeading = "...or a Google Nest Hub Max...",
                    TextColour = Color.Black,
                    LinkText = "SKIP INTRO",
                    textAlignment = TextAlignment.Start
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "Get on the top of the leaderboard and win one of the MI Wirst bands",
                    Image = "prize_miband",
                    MainHeading = "Earn Rewards",
                    SubHeading = "...or a MI Band 4",
                    TextColour = Color.Black,
                    LinkText = "SKIP INTRO",
                    textAlignment = TextAlignment.Start
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "A free audit on your .NET CORE and/or your Angular application ",
                    Image = "prize_consultation",
                    MainHeading = "Earn Rewards",
                    SubHeading = "Free Consultation",
                    TextColour = Color.Black,
                    LinkText = "SKIP INTRO",
                    textAlignment = TextAlignment.Start
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "Win a free place at one of our Superpowers tours events.",
                    Image = "prize_superpowers",
                    MainHeading = "Earn Rewards",
                    SubHeading = "Superpowers Tours",
                    TextColour = Color.Black,
                    LinkText = "GET STARTED",
                    textAlignment = TextAlignment.End
                }
            };

            SetDetails();
        }

        private async void GetStarted()
        {
            if(await _userService.IsLoggedInAsync())
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

        private void SetDetails()
        {
            int itemIndex = SelectedItem;
            MainHeading = Items[itemIndex].MainHeading;
            SubHeading = Items[itemIndex].SubHeading;
            Content = Items[itemIndex].Content;
            BackgroundColour = Items[itemIndex].backgroundColour;
            LinkText = Items[itemIndex].LinkText;
            TextColour = Items[itemIndex].TextColour;
            TextAlignment = Items[itemIndex].textAlignment;
            RaisePropertyChanged(Properties);
        }
    }

    public class CarouselViewModel
    {
        public string MainHeading { get; set; }
        public string SubHeading { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public string LinkText { get; set; }
        public Color backgroundColour { get; set; }
        public Color TextColour { get; set; }
        public TextAlignment textAlignment { get; set; }
    }
}
