using Mopups.Services;

namespace SSW.Rewards.Mobile.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginPageViewModel _viewModel;

    public LoginPage(LoginPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.Navigation = Navigation;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

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
            await MopupService.Instance.PushAsync(new OnBoardingPage(true, statusBarColor as Color));
        }
        else
        {
            await _viewModel.Refresh();
        }
    }
}