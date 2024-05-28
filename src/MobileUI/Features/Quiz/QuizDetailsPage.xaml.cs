using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.Pages;

[QueryProperty(nameof(QuizId), nameof(QuizId))]
public partial class QuizDetailsPage
{
    private QuizDetailsViewModel _viewModel;

    public string QuizId { get; set; }

    public QuizDetailsPage(QuizDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        int quizId = int.Parse(QuizId);
        await _viewModel.Initialise(quizId);
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