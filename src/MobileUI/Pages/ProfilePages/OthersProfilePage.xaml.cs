using SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

namespace SSW.Rewards.Mobile.Pages;

public partial class OthersProfilePage
{
    private OthersProfileViewModel viewModel;
    
    public OthersProfilePage(OthersProfileViewModel vm, int userId)
    {
        InitializeComponent();
        viewModel = vm;

        viewModel.ShowBalance = false;
        
        viewModel.SetUser(userId);

        viewModel.Navigation = Navigation;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        await viewModel.Initialise();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        viewModel.OnDisappearing();
    }
}