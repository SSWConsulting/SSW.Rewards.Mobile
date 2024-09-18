using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class OnBoardingViewModel : BaseViewModel
{
    public ICommand Swiped { get; set; }
    public ObservableRangeCollection<CarouselViewModel> Items { get; set; } = [];
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

    public bool IsFirstRun { get; }

    public EventHandler<int> ScrollToRequested;

    public OnBoardingViewModel(bool isFirstRun)
    {
        IsFirstRun = isFirstRun;
        Swiped = new Command(HandleSwiped);
    }

    public void Initialise()
    {
        Items.AddRange(new List<CarouselViewModel>
        {
            new()
            {
                Heading1 = "Welcome to",
                Heading3 = "REWARDS",
                Image = "ssw_logo_darkmode.png",
            },
            new()
            {
                Heading1 = "EARN POINTS",
                Heading2 = "and network",
                Content = "by scanning QR codes",
                Image = "scan.png",
            },
            new()
            {
                Heading1 = "EARN POINTS",
                Heading2 = "by taking quizzes",
                Image = "credits.png",
            },
            new()
            {
                Heading1 = "EARN PRIZES",
                Heading2 = "buy swag with your points",
                Content = "and win prize giveaways",
                Image = "prizes.png",
            },
            new()
            {
                Heading1 = "GET TO THE TOP",
                Heading2 = "check out the leaderboard",
                Content = "Can you get to the top 10?",
                Image = "position.png",
            },
        });

        SelectedItem = Items[0];
        SetDetails();
    }

    [RelayCommand]
    private async Task ClosePage()
    {
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
        await MopupService.Instance.PopAsync();
        Shell.Current.FlyoutIsPresented = false;

        if (IsFirstRun)
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
