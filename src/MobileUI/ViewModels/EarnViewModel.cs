using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
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
    
    private IDispatcherTimer _timer;
    
    private const int AutoScrollInterval = 6;
    
    [ObservableProperty]
    private bool _isRefreshing;
    
    [ObservableProperty]
    private int _carouselPosition;

    public ObservableCollection<QuizItemViewModel> Quizzes { get; set; } = new ();

    public ObservableCollection<QuizItemViewModel> CarouselQuizzes { get; set; } = new ();

    public EarnViewModel(IQuizService quizService)
    {
        _quizService = quizService;
        WeakReferenceMessenger.Default.Register(this);
        
        _timer = Application.Current.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(AutoScrollInterval);
    }

    public async Task Initialise()
    {
        if (_isLoaded)
            return;

        IsBusy = true;
        await UpdateQuizzes();
        BeginAutoScroll();

        IsBusy = false;
        _isLoaded = true;
    }

    private async Task UpdateQuizzes()
    {
        var quizzes = await _quizService.GetQuizzes();
        
        Quizzes.Clear();
        CarouselQuizzes.Clear();
        _timer.Stop();

        foreach (var quiz in quizzes)
        {
            var vm = new QuizItemViewModel(quiz);
            Quizzes.Add(vm);

            if (quiz.IsCarousel)
            {
                CarouselQuizzes.Add(vm);
            }
        }
        
        CarouselPosition = 0;
        _timer.Start();
    }
    
    private void BeginAutoScroll()
    {
        _timer.Tick += OnScrollTick;
        _timer.Start();
    }
    
    private void OnScrollTick(object sender, object args)
    {
        MainThread.BeginInvokeOnMainThread(Scroll);
    }
    
    private void Scroll()
    {
        var count = CarouselQuizzes.Count;
        
        if (count > 0)
            CarouselPosition = (CarouselPosition + 1) % count;
    }
    
    [RelayCommand]
    private void CarouselScrolled()
    {
        // Reset timer when scrolling
        _timer.Stop();
        _timer.Start();
    }
    
    [RelayCommand]
    private async Task RefreshQuizzes()
    {
        await UpdateQuizzes();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task OpenQuiz(int quizId)
    {
        await AppShell.Current.GoToAsync($"{quizDetailsPageUrl}?QuizId={quizId}");
    }

    private bool CanOpenQuiz(int quizId)
    {
        return Quizzes.First(q => q.Id == quizId).Passed == false;
    }

    public async void Receive(QuizzesUpdatedMessage message)
    {
        IsBusy = true;
        await UpdateQuizzes();
        IsBusy = false;
    }
}

[ObservableObject]
public partial class QuizItemViewModel : QuizDto
{
    [ObservableProperty] private bool _passed;

    public QuizItemViewModel(QuizDto questionDto)
    {
        Id = questionDto.Id;
        Title = questionDto.Title;
        Description = questionDto.Description;
        Passed = questionDto.Passed;
        ThumbnailImage = questionDto.ThumbnailImage;
        CarouselImage = questionDto.CarouselImage;
        IsCarousel = questionDto.IsCarousel;
        Icon = questionDto.Icon;
        Points = questionDto.Points;
    }
}