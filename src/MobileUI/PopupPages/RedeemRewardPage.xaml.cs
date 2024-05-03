using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

namespace SSW.Rewards.Mobile.PopupPages;

public partial class RedeemRewardPage
{
    private readonly RedeemRewardViewModel _viewModel;
    private readonly Reward _reward;
    private readonly Color _parentPageStatusBarColor;

    public RedeemRewardPage(RedeemRewardViewModel viewModel, Reward reward, Color parentPageStatusBarColor = null)
    {
        _parentPageStatusBarColor = parentPageStatusBarColor ?? Colors.Black;
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

    public event EventHandler<object> CallbackEvent;

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CallbackEvent?.Invoke(this, EventArgs.Empty);

        // Change status bar back
        this.Behaviors.Clear();
        this.Behaviors.Add(new StatusBarBehavior
        {
            StatusBarColor = _parentPageStatusBarColor,
            StatusBarStyle = StatusBarStyle.LightContent,
        });
    }
}