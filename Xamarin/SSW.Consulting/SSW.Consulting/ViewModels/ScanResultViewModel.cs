using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Rg.Plugins.Popup.Services;
using SSW.Consulting.Models;
using SSW.Consulting.Services;
using Xamarin.Forms;

namespace SSW.Consulting.ViewModels
{
    public class ScanResultViewModel : BaseViewModel
    {
        public string AnimationRef { get; set; }
        public bool AnimationLoop { get; set; }
        public string ResultHeading { get; set; }
        public string ResultBody { get; set; }
        public string AchievementHeading { get; set; }
        public ICommand OnOkCommand { get; set; }
        public Color HeadingColour { get; set; }
        private IUserService _userService { get; set; }
        private IChallengeService _challengeService { get; set; }

        private bool _wonPrize { get; set; }

        public ScanResultViewModel(string scanData, IUserService userService)
        {
            OnOkCommand = new Command(DismissPopups);
            _userService = userService;
            _challengeService = Resolver.Resolve<IChallengeService>();
            CheckScanData(scanData);
        }

        private async void CheckScanData(string data)
        {
            AnimationRef = "qr-code-scanner.json";
            AnimationLoop = true;
            ResultHeading = "Verifying your QR code...";
            RaisePropertyChanged("AnimationRef", "ResultHeading", "AnimationLoop");

            ChallengeResultViewModel result = await _challengeService.PostChallengeAsync(data);

            AnimationLoop = false;

            switch (result.result)
            {
                case ChallengeResult.Added:
                    AnimationRef = "trophy.json";
                    ResultHeading = "Achivement Added!";
                    ResultBody = string.Format("You have earned {0} points for this achivement", result.Points.ToString());
                    HeadingColour = (Color)Application.Current.Resources["PointsColour"];
                    AchievementHeading = result.Title;
                    _wonPrize = true;
                    MessagingCenter.Send<object>(this, "NewAchievement");
                    break;
                case ChallengeResult.Duplicate:
                    AnimationRef = "judgement.json";
                    ResultHeading = "Already Scanned!";
                    ResultBody = "Are you scanning a bit too aggressively?";
                    AchievementHeading = string.Empty;
                    HeadingColour = Color.White;
                    _wonPrize = false;
                    break;
                case ChallengeResult.NotFound:
                    AnimationRef = "empty-box.json";
                    ResultHeading = "Oops...";
                    ResultBody = "Is this one of our codes? Have you already scanned it?";
                    AchievementHeading = string.Empty;
                    _wonPrize = false;
                    HeadingColour = Color.White;
                    break;
            }

            RaisePropertyChanged(new string[] { "AnimationRef", "AnimationLoop", "ResultHeading", "ResultBody", "PointsColour", "HeadingColour", "AchievementHeading" });

            if (result.result == ChallengeResult.Added)
                await CollectNewPointsAsync();
        }

        private async void DismissPopups()
        {
            if(_wonPrize)
            {
                await Shell.Current.GoToAsync("//main");
                await PopupNavigation.Instance.PopAllAsync();
            }
            else
            {
                await PopupNavigation.Instance.PopAllAsync();
            }
        }

        private async Task CollectNewPointsAsync()
        {
            await _userService.UpdateMyDetailsAsync();
        }
    }
}
