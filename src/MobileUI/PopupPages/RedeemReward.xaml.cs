using Mopups.Pages;

namespace SSW.Rewards.Mobile.PopupPages;

public partial class RedeemReward
{
    private readonly RedeemRewardViewModel _viewModel;
    private readonly Reward _reward;

    public RedeemReward(RedeemRewardViewModel viewModel, Reward reward)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _reward = reward;
        BindingContext = _viewModel;
    }
        
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Initialise(_reward);
    }
}