using Rg.Plugins.Popup.Services;
using SSW.Rewards.Models;
using SSW.Rewards.Pages;
using SSW.Rewards.Services;
using System.Threading.Tasks;
using System.Windows.Input;
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
        
        private IScannerService _scannerService { get; set; }

        private bool _wonPrize { get; set; }

        private string data;

        public ScanResultViewModel(string scanData)
        {
            OnOkCommand = new Command(async () => await DismissPopups());

            _userService = Resolver.Resolve<IUserService>();
            _scannerService = Resolver.Resolve<IScannerService>();

            data = scanData;
        }

        public async Task CheckScanData()
        {
            AnimationRef = "qr-code-scanner.json";
            AnimationLoop = true;
            ResultHeading = "Verifying your QR code...";
            RaisePropertyChanged("AnimationRef", "ResultHeading", "AnimationLoop");

            ScanResponseViewModel result = await _scannerService.ValidateQRCodeAsync(data);

            AnimationLoop = false;

            switch (result.result)
            {
                case ScanResult.Added:
                    if(result.ScanType == ScanType.Achievement)
                    {
                        ResultHeading = "Achivement Added!";
                        AnimationRef = "star.json";
                        ResultBody = string.Format("You have earned ⭐ {0} points for this achivement", result.Points.ToString());
                    }
                    else if(result.ScanType == ScanType.Reward)
                    {
                        ResultHeading = "Congratulations!";
                        AnimationRef = "trophy.json";
                        ResultBody = string.Format("You have claimed this reward!");
                    }
                    
                    HeadingColour = (Color)Application.Current.Resources["PointsColour"];
                    AchievementHeading = result.Title;
                    _wonPrize = true;
                    MessagingCenter.Send<object>(this, ScannerService.PointsAwardedMessage); // TODO: dependency on concretion here
                    break;

                case ScanResult.Duplicate:
                    AnimationRef = "rapid-scan.json";
                    AnimationLoop = true;
                    ResultHeading = "Already Scanned!";
                    ResultBody = "Did you re-scan that accidentally?";
                    AchievementHeading = string.Empty;
                    HeadingColour = Color.White;
                    _wonPrize = false;
                    break;

                case ScanResult.NotFound:
                    AnimationRef = "empty-box.json";
                    ResultHeading = "Unrecognised";
                    ResultBody = "This doesn't look like an SSW code";
                    AchievementHeading = string.Empty;
                    _wonPrize = false;
                    HeadingColour = Color.White;
                    break;

                case ScanResult.InsufficientBalance:
                    AnimationRef = "coin.json";
                    ResultHeading = "Not Enough Points";
                    ResultBody = "You do not have enough points to claim this reward yet. Try again later.";
                    AchievementHeading = string.Empty;
                    _wonPrize = false;
                    HeadingColour = Color.White;
                    break;

                case ScanResult.Error:
                    AnimationRef = "empty-box.json";
                    ResultHeading = "It's not you it's me...";
                    ResultBody = "Something went wrong there. Please try again.";
                    AchievementHeading = string.Empty;
                    _wonPrize = false;
                    HeadingColour = Color.White;
                    break;
            }

            RaisePropertyChanged(new string[] { "AnimationRef", "AnimationLoop", "ResultHeading", "ResultBody", "PointsColour", "HeadingColour", "AchievementHeading" });

            if (result.result == ScanResult.Added)
            {
                await _userService.UpdateMyDetailsAsync();
                MessagingCenter.Send(this, ScannerService.PointsAwardedMessage); // TODO: dependency on concretion here
            }
        }

        private async Task DismissPopups()
        {
            if(_wonPrize)
            {
                await DismissWithWon();
            }
            else
            {
                await DismissWithoutWon();
            }
        }

        private async Task DismissWithWon()
        {
            await Shell.Current.GoToAsync("//main");
            await PopupNavigation.Instance.PopAllAsync();
        }

        private async Task DismissWithoutWon()
        {
            MessagingCenter.Send<object>(this, ScanPage.EnableScannerMessage); // TODO: Dependency on page
            await PopupNavigation.Instance.PopAllAsync();
        }
    }
}
