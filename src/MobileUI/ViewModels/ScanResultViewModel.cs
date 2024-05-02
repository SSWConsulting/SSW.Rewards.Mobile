using Mopups.Services;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class ScanResultViewModel : BaseViewModel
{
    private readonly IUserService _userService;

    private readonly IScannerService _scannerService;

    private bool _wonPrize { get; set; }
    private string data;

    [ObservableProperty]
    private string _animationRef;

    [ObservableProperty]
    private bool _animationLoop;

    [ObservableProperty]
    private string _resultHeading;

    [ObservableProperty]
    private string _resultBody;

    [ObservableProperty]
    private string _achievementHeading;

    [ObservableProperty]
    private Color _headingColour;

    public ScanResultViewModel(IUserService userService, IScannerService scannerService)
    {
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
                Application.Current.Resources.TryGetValue("PointsColour", out var color);
                HeadingColour = (Color)color;
                AchievementHeading = result.Title;
                _wonPrize = true;
                break;

            case ScanResult.Confirmed:
                ResultHeading = "Pending redemption confirmed";
                ResultBody = "Please provide the reward to the user.";
                AnimationRef = "trophy.json";
                HeadingColour = Colors.White;
                AchievementHeading = result.Title;
                _wonPrize = true;
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
                ResultBody = "Try scanning this with your phone camera instead.";
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

        if (result.result == ScanResult.Added)
        {
            await _userService.UpdateMyDetailsAsync();
        }
    }

    [RelayCommand]
    private async Task Ok()
    {
        await DismissPopups();
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
        await MopupService.Instance.PopAllAsync();
        await Navigation.PopModalAsync();
    }

    private async Task DismissWithoutWon()
    {
        // Important! We should pop the page before enabling scanner;
        // otherwise PopAllAsync in some cases might also close the next opening popup
        // together with the current one disabling the scanner. https://github.com/SSWConsulting/SSW.Rewards.Mobile/issues/543
        await MopupService.Instance.PopAllAsync();
        WeakReferenceMessenger.Default.Send(new EnableScannerMessage());
    }
}
