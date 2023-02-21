using Mopups.Services;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels;

public class ScanResultViewModel : BaseViewModel
{
    private readonly IUserService _userService;

    private readonly IScannerService _scannerService;
    
    private bool _wonPrize { get; set; }
    private string data;

    public string AnimationRef { get; set; }

    public bool AnimationLoop { get; set; }
    
    public string ResultHeading { get; set; }
    
    public string ResultBody { get; set; }
    
    public string AchievementHeading { get; set; }
    
    public ICommand OnOkCommand { get; set; }
    
    public Color HeadingColour { get; set; }

    public ScanResultViewModel(IUserService userService, IScannerService scannerService)
    {
        OnOkCommand = new Command(async () => await DismissPopups());

        _userService = userService;
        _scannerService = scannerService;
    }

    public void SetScanData(string data)
    {
        this.data = data;
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
                    ResultHeading = "Achievement Added!";
                    AnimationRef = "star.json";
                    ResultBody = $"You have earned ⭐ {result.Points.ToString()} points for this achievement";
                }
                else if(result.ScanType == ScanType.Reward)
                {
                    ResultHeading = "Congratulations!";
                    AnimationRef = "trophy.json";
                    ResultBody = "You have claimed this reward!";
                }
                
                HeadingColour = (Color)Application.Current.Resources["PointsColour"];
                AchievementHeading = result.Title;
                _wonPrize = true;
                WeakReferenceMessenger.Default.Send(new PointsAwardedMessage());
                break;

            case ScanResult.Duplicate:
                AnimationRef = "rapid-scan.json";
                AnimationLoop = true;
                ResultHeading = "Already Scanned!";
                ResultBody = "Did you re-scan that accidentally?";
                AchievementHeading = string.Empty;
                HeadingColour = Colors.White;
                _wonPrize = false;
                break;

            case ScanResult.NotFound:
                AnimationRef = "empty-box.json";
                ResultHeading = "Unrecognised";
                ResultBody = "This doesn't look like an SSW code";
                AchievementHeading = string.Empty;
                _wonPrize = false;
                HeadingColour = Colors.White;
                break;

            case ScanResult.InsufficientBalance:
                AnimationRef = "coin.json";
                ResultHeading = "Not Enough Points";
                ResultBody = "You do not have enough points to claim this reward yet. Try again later.";
                AchievementHeading = string.Empty;
                _wonPrize = false;
                HeadingColour = Colors.White;
                break;

            case ScanResult.Error:
                AnimationRef = "empty-box.json";
                ResultHeading = "It's not you it's me...";
                ResultBody = "Something went wrong there. Please try again.";
                AchievementHeading = string.Empty;
                _wonPrize = false;
                HeadingColour = Colors.White;
                break;
        }

        RaisePropertyChanged(new[] { "AnimationRef", "AnimationLoop", "ResultHeading", "ResultBody", "PointsColour", "HeadingColour", "AchievementHeading" });

        if (result.result == ScanResult.Added)
        {
            await _userService.UpdateMyDetailsAsync();
            WeakReferenceMessenger.Default.Send(new PointsAwardedMessage());
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
        await MopupService.Instance.PopAllAsync();
    }

    private async Task DismissWithoutWon()
    {
        WeakReferenceMessenger.Default.Send(new EnableScannerMessage());
        await MopupService.Instance.PopAllAsync();
    }
}
