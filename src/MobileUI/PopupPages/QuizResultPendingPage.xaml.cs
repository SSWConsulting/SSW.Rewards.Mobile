
namespace SSW.Rewards.Mobile.PopupPages;

public partial class QuizResultPendingPage
{
    private readonly QuizResultPendingViewModel _viewModel;

    public QuizResultPendingPage(QuizResultPendingViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    protected override void OnDisappearing()
    {
        _viewModel.KillTimer();
    }
}
