using Maui.Plugins.PageResolver;

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
                LogoImage.ScaleTo(3, 1000, Easing.CubicIn)

            );

        if (Preferences.Get("FirstRun", true))
        {
            Preferences.Set("FirstRun", false);
            await Navigation.PushModalAsync<OnBoarding>();
        }
        else
        {
            await _viewModel.Refresh();
        }
    }
}