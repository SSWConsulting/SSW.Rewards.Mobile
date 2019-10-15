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
        public string ResultHeading { get; set; }
        public string ResultBody { get; set; }
        public string AchievementHeading { get; set; }
        public ICommand OnOkCommand { get; set; }
        public Color HeadingColour { get; set; }
        private IUserService _userService { get; set; }

        public ScanResultViewModel(ChallengeResultViewModel result, IUserService userService)
        {
            OnOkCommand = new Command(DismissPopups);
            _userService = userService;

            switch (result.result)
            {
                case ChallengeResult.Added:
                    AnimationRef = "trophy.json";
                    ResultHeading = "Achivement Added!";
                    ResultBody = string.Format("You have earned {0} points for this achivement", result.Points.ToString());
                    HeadingColour = (Color)Application.Current.Resources["PointsColour"];
                    AchievementHeading = result.Title;
                    MessagingCenter.Send<object>(this, "NewAchievement");
                    break;
                case ChallengeResult.Duplicate:
                    AnimationRef = "judgement.json";
                    ResultHeading = "Already Scanned!";
                    ResultBody = "Are you scanning a bit too aggressively?";
                    AchievementHeading = string.Empty;
                    HeadingColour = Color.White;
                    break;
                case ChallengeResult.NotFound:
                    AnimationRef = "empty-box.json";
                    ResultHeading = "Oops...";
                    ResultBody = "Is this one of our codes? Have you already scanned it?";
                    AchievementHeading = string.Empty;
                    HeadingColour = Color.White;
                    break;
            }

            RaisePropertyChanged(new string[] { "AnimationRef", "ResultHeading", "ResultBody", "PointsColour", "HeadingColour", "AchievementHeading" });

            if (result.result == ChallengeResult.Added)
                CollectNewPointsAsync();
        }

        private async void DismissPopups()
        {
            await PopupNavigation.Instance.PopAllAsync();
        }

        private async Task CollectNewPointsAsync()
        {
            await _userService.UpdateMyDetailsAsync();
        }
    }
}
