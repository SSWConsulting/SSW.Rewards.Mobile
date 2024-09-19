using Mopups.Services;

namespace SSW.Rewards.Mobile.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginPageViewModel _viewModel;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public LoginPage(LoginPageViewModel viewModel, IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.Navigation = Navigation;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("LoginPage");

        await Task.WhenAny<bool>
            (
                LogoImage.TranslateTo(0, -200, 1000, Easing.CubicIn),
                LogoImage.ScaleTo(1, 1000, Easing.CubicIn)
            );
        _viewModel.LoginButtonEnabled = true;

        if (Preferences.Get("FirstRun", true))
        {
            Preferences.Set("FirstRun", false);
            Application.Current.Resources.TryGetValue("SecondaryBackground", out var statusBarColor);
            await MopupService.Instance.PushAsync(new OnBoardingPage(_firebaseAnalyticsService, true, statusBarColor as Color));
        }
        else
        {
            await _viewModel.Refresh();
        }
    }

    public void QueueCodeScan(string code)
    {
        _viewModel.QueueCodeScan(code);
    }
}