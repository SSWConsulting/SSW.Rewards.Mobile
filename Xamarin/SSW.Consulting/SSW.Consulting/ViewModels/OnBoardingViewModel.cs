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
                    SubHeading = "Earn rewards",
                    TextColour = Color.Black,
                    LinkText = "SKIP INTRO",
                    textAlignment = TextAlignment.Start
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "Get on the leaderboard for a chance to win a Google Hub Max.",
                    Image = "prize_hubmax",
                    MainHeading = "Earn Rewards",
                    SubHeading = "Google Nest Hub Max",
                    TextColour = Color.Black,
                    LinkText = "SKIP INTRO",
                    textAlignment = TextAlignment.Start
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "Earn enough points and you could claim a smart water bottle with touch activated content thermometer.",
                    Image = "prize_keepcup",
                    MainHeading = "Earn Rewards",
                    SubHeading = "SSW Smart Keepcup",
                    TextColour = Color.Black,
                    LinkText = "SKIP INTRO",
                    textAlignment = TextAlignment.Start
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "Get on the leaderboard and earn a MI Wrist band. Just like a FitBit, except more functionality and a month's battery life!",
                    Image = "prize_miband",
                    MainHeading = "Earn Rewards",
                    SubHeading = "MI Band 4",
                    TextColour = Color.Black,
                    LinkText = "SKIP INTRO",
                    textAlignment = TextAlignment.Start
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "SSW Architects will help you successfully implement your project.",
                    Image = "prize_consultation",
                    MainHeading = "Earn Rewards",
                    SubHeading = "Half Price Specification Review",
                    TextColour = Color.Black,
                    LinkText = "SKIP INTRO",
                    textAlignment = TextAlignment.Start
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "Win a free place at one of our Superpowers tours events. You can choose to attend to our .NET Core, Angular or Azure one-day training.",
                    Image = "prize_superpowers",
                    MainHeading = "Earn Rewards",
                    SubHeading = "SuperPowers ticket",
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
