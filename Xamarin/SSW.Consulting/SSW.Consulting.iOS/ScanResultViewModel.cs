using System;
using System.Windows.Input;
using Rg.Plugins.Popup.Services;
using SSW.Consulting.Models;
using Xamarin.Forms;

namespace SSW.Consulting.ViewModels
{
    public class ScanResultViewModel : BaseViewModel
    {
        public string AnimationRef { get; set; }
        public string ResultHeading { get; set; }
        public string ResultBody { get; set; }
        public ICommand OnOkCommand { get; set; }
        public Color HeadingColour { get; set; }

        public ScanResultViewModel(ChallengeResult result)
        {
            switch(result)
            {
                case ChallengeResult.Added:
                    AnimationRef = "trophy.json";
                    ResultHeading = "Achivement Added!";
                    ResultBody = "You have got the points for this achivement";
                    HeadingColour = (Color)Application.Current.Resources["PointsColour"];
                    break;
                case ChallengeResult.Duplicate:
                    AnimationRef = "judgement.json";
                    ResultHeading = "Already Scanned!";
                    ResultBody = "Are you scanning a bit too aggressively?";
                    HeadingColour = Color.White;
                    break;
                case ChallengeResult.NotFound:
                    AnimationRef = "empty-box.json";
                    ResultHeading = "Oops...";
                    ResultBody = "This doesn't look like one of our codes!";
                    HeadingColour = Color.White;
                    break;
            }

            RaisePropertyChanged(new string[] { "AnimationRef", "ResultHeading", "ResultBody", "PointsColour" } );
        }

        private async void DismissPopups()
        {
            await PopupNavigation.Instance.PopAllAsync();
        }
    }
}
