using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

namespace SSW.Rewards.Mobile.PopupPages;

public partial class RedeemRewardPage
{
    private readonly RedeemRewardViewModel _viewModel;
    private readonly Reward _reward;
    private readonly Color _parentPageStatusBarColor;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public RedeemRewardPage(IFirebaseAnalyticsService firebaseAnalyticsService, RedeemRewardViewModel viewModel, Reward reward, Color parentPageStatusBarColor = null)
    {
        _firebaseAnalyticsService = firebaseAnalyticsService;
        _parentPageStatusBarColor = parentPageStatusBarColor ?? Colors.Black;
        InitializeComponent();
        _viewModel = viewModel;
        _reward = reward;
        BindingContext = _viewModel;
        _viewModel.ViewPage = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("RedeemRewardPage");
        _viewModel.Initialise(_reward);
    }

    public event EventHandler<object> CallbackEvent;

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
        if (_viewModel.ShouldCallCallback)
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