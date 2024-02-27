
namespace SSW.Rewards.Mobile.PopupPages;

public partial class QuizResultPendingPage
{
    private readonly QuizResultPendingViewModel _viewModel;

    public QuizResultPendingPage(QuizResultPendingViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.TimeLapsed += OnTimeLapsed;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    private int _angle;
    private async void OnTimeLapsed()
    {
        _angle = 5 * 360;
        await ChatGptIcon.RotateTo(360.0, 250, Easing.CubicOut);
        ChatGptIcon.Rotation = 0;
    }

    protected override void OnDisappearing()
    {
        _viewModel.TimeLapsed -= OnTimeLapsed;
        _viewModel.KillTimer();
    }
}
