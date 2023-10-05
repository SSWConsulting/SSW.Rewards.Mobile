using SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

namespace SSW.Rewards.Mobile.Pages;

public partial class MyProfilePage : ContentPage
{
    private bool _initialised;

    private MyProfileViewModel viewModel;


    public MyProfilePage(MyProfileViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        viewModel.Navigation = Navigation;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        if (!_initialised)
            await viewModel.Initialise();

        _initialised = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        viewModel.OnDisappearing();
    }
}