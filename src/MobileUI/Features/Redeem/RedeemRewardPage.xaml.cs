namespace SSW.Rewards.Mobile.PopupPages;

public partial class RedeemRewardPage
{
    private readonly RedeemRewardViewModel _viewModel;
    private readonly Reward _reward;
    private readonly int _userBalance;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public RedeemRewardPage(IFirebaseAnalyticsService firebaseAnalyticsService, RedeemRewardViewModel viewModel, Reward reward, int userBalance)
    {
        _firebaseAnalyticsService = firebaseAnalyticsService;
        InitializeComponent();
        _viewModel = viewModel;
        _reward = reward;
        _userBalance = userBalance;
        BindingContext = _viewModel;
        _viewModel.ViewPage = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("RedeemRewardPage");
        _viewModel.Initialise(_reward, _userBalance);
    }

    public event EventHandler<object> CallbackEvent;

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
        if (_viewModel.ShouldCallCallback)
            CallbackEvent?.Invoke(this, EventArgs.Empty);
    }
}
