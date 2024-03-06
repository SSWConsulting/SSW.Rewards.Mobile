using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class EarnViewModel : BaseViewModel, IRecipient<QuizzesUpdatedMessage>
{
    private bool _isLoaded;
    private readonly IQuizService _quizService;

    private string quizDetailsPageUrl = "earn/details";

    public ObservableCollection<QuizDto> Quizzes { get; set; } = new ();

    public ObservableCollection<QuizDto> CarouselQuizzes { get; set; } = new ();

    public EarnViewModel(IQuizService quizService)
    {
        _quizService = quizService;
        WeakReferenceMessenger.Default.Register(this);
    }

    public async Task Initialise()
    {
        if (_isLoaded)
        {
            return;
        }

        Quizzes = new ObservableCollection<QuizDto>();
        OnPropertyChanged(nameof(Quizzes));
        CarouselQuizzes = new ObservableCollection<QuizDto>();
        OnPropertyChanged(nameof(CarouselQuizzes));

        IsBusy = true;

        var quizzes = await _quizService.GetQuizzes();

        foreach (var quiz in quizzes)
        {
            Quizzes.Add(quiz);

            if (quiz.IsCarousel)
            {
                CarouselQuizzes.Add(quiz);
            }
        }

        IsBusy = false;
        _isLoaded = true;
    }

    [RelayCommand]
    private async Task OpenQuiz(int quizId)
    {
        var quiz = Quizzes.First(q => q.Id == quizId);
        await AppShell.Current.GoToAsync($"{quizDetailsPageUrl}?QuizId={quiz.Id}&QuizIcon={quiz.Icon}");
    }

    private bool CanOpenQuiz(int quizId)
    {
        return Quizzes.First(q => q.Id == quizId).Passed == false;
    }

    public async void Receive(QuizzesUpdatedMessage message)
    {
        await Initialise();
    }
}
