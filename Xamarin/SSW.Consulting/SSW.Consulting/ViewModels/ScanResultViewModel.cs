using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Rg.Plugins.Popup.Services;
using SSW.Rewards.Models;
using SSW.Rewards.Services;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
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
            _ = CheckScanData(scanData);
        }

        private async Task CheckScanData(string data)
        {
            AnimationRef = "qr-code-scanner.json";
            AnimationLoop = true;
            ResultHeading = "Verifying your QR code...";
            RaisePropertyChanged("AnimationRef", "ResultHeading", "AnimationLoop");

            ChallengeResultViewModel result = await _challengeService.ValidateQRCodeAsync(data);

            AnimationLoop = false;

            switch (result.result)
            {
                case ChallengeResult.Added:
                    if(result.ChallengeType == ChallengeType.Achievement)
                    {
                        ResultHeading = "Achivement Added!";
                        ResultBody = string.Format("You have earned ⭐ {0} points for this achivement", result.Points.ToString());
                    }
                    else if(result.ChallengeType == ChallengeType.Reward)
                    {
                        ResultHeading = "Congratulations!";
                        ResultBody = string.Format("You have claimed this reward!");
                    }
                    AnimationRef = "trophy.json";
                    HeadingColour = (Color)Application.Current.Resources["PointsColour"];
                    AchievementHeading = result.Title;
                    _wonPrize = true;
                    MessagingCenter.Send<object>(this, "NewAchievement");
                    break;
                case ChallengeResult.Duplicate:
                    AnimationRef = "rapid-scan.json";
                    AnimationLoop = true;
                    ResultHeading = "Already Scanned!";
                    ResultBody = "Are you scanning a bit too aggressively?";
                    AchievementHeading = string.Empty;
                    HeadingColour = Color.White;
                    _wonPrize = false;
                    break;
                case ChallengeResult.NotFound:
                    AnimationRef = "empty-box.json";
                    ResultHeading = "Unrecognised";
                    ResultBody = "This doesn't look like an SSW code";
                    AchievementHeading = string.Empty;
                    _wonPrize = false;
                    HeadingColour = Color.White;
                    break;
                case ChallengeResult.Error:
                    AnimationRef = "empty-box.json";
                    ResultHeading = "It's not you it's me...";
                    ResultBody = "Something went wrong there. Please try again.";
                    AchievementHeading = string.Empty;
                    _wonPrize = false;
                    HeadingColour = Color.White;
                    break;
            }

            RaisePropertyChanged(new string[] { "AnimationRef", "AnimationLoop", "ResultHeading", "ResultBody", "PointsColour", "HeadingColour", "AchievementHeading" });

            if (result.result == ChallengeResult.Added)
                await CollectNewPointsAsync();
        }

        private void DismissPopups()
        {
            if(_wonPrize)
            {
                _ = DismissWithWon();
            }
            else
            {
                _ = DismissWithoutWon();
            }
        }

        private async Task DismissWithWon()
        {
            await Shell.Current.GoToAsync("//main");
            await PopupNavigation.Instance.PopAllAsync();
        }

        private async Task DismissWithoutWon()
        {
            await PopupNavigation.Instance.PopAllAsync();
        }

        private async Task CollectNewPointsAsync()
        {
            await _userService.UpdateMyDetailsAsync();
        }
    }
}
