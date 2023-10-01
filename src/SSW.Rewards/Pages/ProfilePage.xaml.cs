using SSW.Rewards.Mobile.Controls;

namespace SSW.Rewards.Mobile.Pages;

public partial class ProfilePage : ContentPage
{
    private bool _isMe;

    private bool _initialised;

    private ProfileViewModel viewModel;


    public ProfilePage(ProfileViewModel vm)//, bool isMe = true)
    {
        InitializeComponent();
        viewModel = vm;
        viewModel.Navigation = Navigation;
        BindingContext = viewModel;

        _isMe = true;
    }

    public ProfilePage(IRewardService rewardsService, IUserService userService, LeaderViewModel leader)
    {
        InitializeComponent();
        // HACK: Need to find a better way to handle these two differnt constructors
        viewModel = new ProfileViewModel(rewardsService, userService, leader);
        viewModel.Navigation = Navigation;
        BindingContext = viewModel;

        _isMe = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_isMe && _initialised)
        {

        }
        else
        {
            await viewModel.Initialise(_isMe);
        }
        viewModel.ShowSnackbar += ShowSnackbar;
        _initialised = true;
    }

    private async void ShowSnackbar(object sender, ShowSnackbarEventArgs e)
    {
        ProfilePageSnackbar.Options = e.Options;
        await ProfilePageSnackbar.ShowSnackbar();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        //viewModel.ShowSnackbar -= ShowSnackbar;
        viewModel.OnDisappearing();
    }
}