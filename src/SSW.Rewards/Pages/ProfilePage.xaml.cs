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

    public ProfilePage(IRewardService rewardsService, IUserService userService, ISnackbarService snackbarService, LeaderViewModel leader)
    {
        InitializeComponent();
        // HACK: Need to find a better way to handle these two different constructors
        viewModel = new ProfileViewModel(rewardsService, userService, snackbarService, leader);
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
        _initialised = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        viewModel.OnDisappearing();
    }
}