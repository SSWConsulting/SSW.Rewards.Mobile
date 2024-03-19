using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class OnBoardingViewModel : BaseViewModel
{
    public ICommand Swiped { get; set; }
    public ObservableCollection<CarouselViewModel> Items { get; set; }
    public CarouselViewModel SelectedItem { get; set; }

    [ObservableProperty]
    private string _heading1;

    [ObservableProperty]
    private string _heading2;

    [ObservableProperty]
    private string _heading3;

    [ObservableProperty]
    private string _content;

    [ObservableProperty]
    private bool _isFirst;

    [ObservableProperty]
    private bool _isLast;

    [ObservableProperty]
    private bool _isOverlayVisible;

    public bool IsFirstRun { get; }

    public EventHandler<int> ScrollToRequested;

    public OnBoardingViewModel(bool isFirstRun)
    {
        IsFirstRun = isFirstRun;
        Swiped = new Command(HandleSwiped);
        Items = new ObservableCollection<CarouselViewModel>
        {
            new()
            {
                Heading1 = "Welcome to",
                Heading3 = "REWARDS",
                Image = "ssw_logo_darkmode.png",
            },
            new()
            {
                Heading1 = "EARN CREDITS",
                Heading2 = "By Taking Quizzes",
                Image = "credits.png",
            },
            new()
            {
                Heading1 = "NETWORK",
                Heading2 = "By Scanning QR Codes",
                Content = "And earn up to 1000 credits per scan",
                Image = "scan.png",
            },
            new()
            {
                Heading1 = "GET REWARDED",
                Heading2 = "Convert Credits to Prizes",
                Content = "Convert your credits to awesome Prizes",
                Image = "prizes.png",
            },
            new()
            {
                Heading1 = "COMPETE!",
                Heading2 = "Build Your Rank",
                Content = "Move up the leaderboard",
                Image = "position.png",
            },
        };

        SelectedItem = Items[0];

        SetDetails();
    }

    [RelayCommand]
    private async Task ClosePage()
    {
        IsOverlayVisible = false;
        await MopupService.Instance.PopAsync();
    }

    private void HandleSwiped()
    {
        if (SelectedItem is null)
            return;
        SetDetails();
    }

    [RelayCommand]
    private void SwipeForward()
    {
        var nextIndex = Items.IndexOf(SelectedItem) + 1;
        ScrollToRequested.Invoke(null, nextIndex);
    }

    [RelayCommand]
    private void SwipeBackward()
    {
        var prevIndex = Items.IndexOf(SelectedItem) - 1;
        ScrollToRequested.Invoke(null, prevIndex);
    }

    [RelayCommand]
    private async Task GetStarted()
    {
        IsOverlayVisible = false;
        await MopupService.Instance.PopAsync();
        Shell.Current.FlyoutIsPresented = false;
        await Shell.Current.GoToAsync("//earn");
    }

    private void SetDetails()
    {
        Heading1 = SelectedItem.Heading1;
        Heading2 = SelectedItem.Heading2;
        Heading3 = SelectedItem.Heading3;
        Content = SelectedItem.Content;
        IsFirst = Items.IndexOf(SelectedItem) == 0;
        IsLast = Items.IndexOf(SelectedItem) == Items.Count - 1;
    }
}

public class CarouselViewModel
{
    public string Heading1 { get; set; }
    public string Heading2 { get; set; }
    public string Heading3 { get; set; }
    public string Content { get; set; }
    public string Image { get; set; }
}
