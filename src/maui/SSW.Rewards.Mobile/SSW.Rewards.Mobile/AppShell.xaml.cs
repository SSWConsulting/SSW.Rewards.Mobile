using System.Diagnostics;
using Mopups.Services;
using SSW.Rewards.PopupPages;

namespace SSW.Rewards.Mobile;


public partial class AppShell : Shell
{
    private readonly IUserService _userService;

    //private string _name;
    //private string _email;
    //private string _profilePic;

    public bool IsIosVisible { get; set; } = false;
    public bool IsAndroidVisible { get; set; } = false;

    public AppShell(IUserService userService, bool isStaff)
    {
        IsStaff = isStaff;

        BindingContext = this;
        InitializeComponent();
        _userService = userService;
        VersionLabel.Text = $"Version {AppInfo.VersionString}";
        Routing.RegisterRoute("quiz/details", typeof(QuizDetailsPage));

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            IsAndroidVisible = true;
        }

        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            IsIosVisible = true;
        }
    }

    private bool _isStaff;
    public bool IsStaff
    {
        get => _isStaff;
        set
        {
            _isStaff = value;
            OnPropertyChanged();
        }
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);
        if (args.Source is ShellNavigationSource.ShellSectionChanged or ShellNavigationSource.ShellItemChanged)
        {
            UpdateTabIconColorOniOS();
        }
    }

    public async void Handle_LogOutClicked(object sender, EventArgs e)
    {
        var sure = await App.Current.MainPage.DisplayAlert("Logout", "Are you sure you want to log out of SSW Rewards?", "Yes", "No");

        if (sure)
        {
            _userService.SignOut();
            await Navigation.PushModalAsync<LoginPage>();
        }
    }

    public void Handle_EventsClicked(object sender, EventArgs e)
    {
        var popup = new Events();
        MopupService.Instance.PushAsync(popup);
    }

    public void Handle_JoinClicked(object sender, EventArgs e)
    {
        var popup = new JoinUs();
        MopupService.Instance.PushAsync(popup);
    }

    public void Handle_AboutClicked(object sender, EventArgs e)
    {
        var popup = new AboutSSW();
        MopupService.Instance.PushAsync(popup);
    }

    public void Handle_HowToPlayClicked(object sender, EventArgs e)
    {
        Navigation.PushModalAsync<OnBoarding>();
    }

    private void Handle_QRClicked(object sender, EventArgs e)
    {
        var qrCode = _userService.MyQrCode;

        if (!string.IsNullOrWhiteSpace(qrCode))
        {
            var popup = new MyQRPage(qrCode);
            MopupService.Instance.PushAsync(popup);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (Application.Current.MainPage.GetType() == typeof(AppShell) && Shell.Current.Navigation.NavigationStack.Where(x => x != null).Any())
        {
            return base.OnBackButtonPressed();
        }
        
        Process.GetCurrentProcess().CloseMainWindow();
        return true;
    }

    /// <summary>
    /// TODO: MAUI TabbedPage bug on iOS https://github.com/dotnet/maui/issues/12250
    /// Remove when the bug is fixed
    /// </summary>
    [Conditional("IOS")]
    private void UpdateTabIconColorOniOS()
    {
        foreach (var shellSection in MyTabbar.Items)
        {
            var img = (FontImageSource)shellSection.Icon;
            var isCurrentPage = MyTabbar.CurrentItem == shellSection;
            shellSection.Icon = new FontImageSource
            {
                Color = isCurrentPage ? Color.FromRgba("#BE4b47") : Color.FromArgb("#95FFFFFF"),
                Glyph = img.Glyph,
                FontFamily = img.FontFamily,
            };
        }
    }
}