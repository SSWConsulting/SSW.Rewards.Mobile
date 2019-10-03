using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

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
        public Color BackgroundColour { get; set; }
        public Color TextColour { get; set; }
        public string[] Properties { get; set; }

        public OnBoardingViewModel()
        {
            GetStartedTapped = new Command(GetStarted);
            Swiped = new Command(SetDetails);
            Properties = new string[] { "MainHeading", "SubHeading", "Content", "BackgroundColour", "TextColour" };
            Items = new ObservableCollection<CarouselViewModel>
            {
                new CarouselViewModel
                {
                    backgroundColour = (Color) Application.Current.Resources["SSWRed"],
                    Content = "Talk to SSW people, attend talks QR codes will be shown at the end of the presentations or complete the techquiz in this app.",
                    Image = "onboarding",
                    MainHeading = "Earning Points",
                    SubHeading = "How to get them?",
                    TextColour = Color.White
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "Get on the top of the leaderboard and win a Google Hub Max or one of the MI Wirst bands",
                    Image = "prize_googlehome",
                    MainHeading = "Earning Rewards",
                    SubHeading = "Google Home",
                    TextColour = Color.Black
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "Get on the top of the leaderboard and win a Google Hub Max or one of the MI Wirst bands",
                    Image = "prize_hubmax",
                    MainHeading = "Earning Rewards",
                    SubHeading = "Google Hub Max",
                    TextColour = Color.Black
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "Get on the top of the leaderboard and win a Google Hub Max or one of the MI Wirst bands",
                    Image = "prize_miband",
                    MainHeading = "Earning Rewards",
                    SubHeading = "MI Band",
                    TextColour = Color.Black
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "A free audit on your .NET CORE and/or your Angular application ",
                    Image = "prize_consultation",
                    MainHeading = "Earning Rewards",
                    SubHeading = "Free Consultation",
                    TextColour = Color.Black
                },
                new CarouselViewModel
                {
                    backgroundColour = Color.White,
                    Content = "Get on the top of the leaderboard and win a Google Hub Max or one of the MI Wirst bands",
                    Image = "prize_superpowers",
                    MainHeading = "Earning Rewards",
                    SubHeading = ".NET CORE Superpowers",
                    TextColour = Color.Black
                }
            };

            SetDetails();
        }

        private void GetStarted()
        {
            AppShell shell = new AppShell();
            Application.Current.MainPage = shell;
        }

        private void SetDetails()
        {
            int itemIndex = SelectedItem;
            MainHeading = Items[itemIndex].MainHeading;
            SubHeading = Items[itemIndex].SubHeading;
            Content = Items[itemIndex].Content;
            BackgroundColour = Items[itemIndex].backgroundColour;
            TextColour = Items[itemIndex].TextColour;
            RaisePropertyChanged(Properties);
        }
    }

    public class CarouselViewModel
    {
        public string MainHeading { get; set; }
        public string SubHeading { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public Color backgroundColour { get; set; }
        public Color TextColour { get; set; }
    }
}
