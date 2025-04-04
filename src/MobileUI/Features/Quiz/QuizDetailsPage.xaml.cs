using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.Pages;

public partial class QuizDetailsPage
{
    private QuizDetailsViewModel _viewModel;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    private readonly int _quizId;

    public QuizDetailsPage(QuizDetailsViewModel viewModel, IFirebaseAnalyticsService firebaseAnalyticsService, int quizId)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        BindingContext = _viewModel;
        _quizId = quizId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("QuizDetailsPage");
        await _viewModel.Initialise(_quizId);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        if (_viewModel.TestPassed)
        {
            WeakReferenceMessenger.Default.Send(new QuizzesUpdatedMessage());
        }
    }
}