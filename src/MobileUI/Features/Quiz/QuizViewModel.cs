using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class QuizViewModel : BaseViewModel, IRecipient<QuizzesUpdatedMessage>
{
    private readonly IQuizService _quizService;
    private readonly IServiceProvider _provider;
    private readonly IFileCacheService _fileCacheService;
    private readonly IDispatcherTimer _timer;
    
    private const int AutoScrollInterval = 6;
    private const string CacheKey = "QuizzesList";
    
    [ObservableProperty]
    private bool _isRefreshing;
    
    [ObservableProperty]
    private int _carouselPosition;

    public AdvancedObservableCollection<QuizItemViewModel> Quizzes { get; } = new();
    public ObservableRangeCollection<QuizItemViewModel> CarouselQuizzes { get; set; } = [];

    public QuizViewModel(IQuizService quizService, IServiceProvider provider, IFileCacheService fileCacheService)
    {
        _quizService = quizService;
        _provider = provider;
        _fileCacheService = fileCacheService;
        WeakReferenceMessenger.Default.Register(this);
        
        _timer = Application.Current.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(AutoScrollInterval);
    }

    public async Task Initialise()
    {
        Quizzes.CompareItems = QuizItemViewModel.IsEqual;
        Quizzes.OnCollectionUpdated += OnQuizzesUpdated;
        Quizzes.OnError += OnQuizzesError;
        Quizzes.InitializeInitialCaching(_fileCacheService, CacheKey, () => true);

        if (!Quizzes.IsLoaded)
        {
            await LoadData();
        }

        BeginAutoScroll();
    }

    private async Task LoadData()
    {
        if (!Quizzes.IsLoaded)
            IsBusy = true;

        _timer.Stop();
        CarouselPosition = 0;

        await Quizzes.LoadAsync(async ct => await FetchQuizzesData(ct), reload: true);
    }

    private async Task<List<QuizItemViewModel>> FetchQuizzesData(CancellationToken ct)
    {
        var quizzes = await _quizService.GetQuizzes();
        var quizDtos = quizzes?.ToList() ?? [];

        var quizzesList = new List<QuizItemViewModel>();

        foreach (var quiz in quizDtos)
        {
            quizzesList.Add(new QuizItemViewModel(quiz));
        }

        return quizzesList;
    }

    private void OnQuizzesUpdated(List<QuizItemViewModel> quizzes, bool isFromCache)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsBusy = false;
            IsRefreshing = false;
            _timer.Start();

            CarouselQuizzes.ReplaceRange(Quizzes.Collection.Where(q => q.IsCarousel));
        });
    }

    private bool OnQuizzesError(Exception ex)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!Quizzes.IsLoaded)
            {
                string userMessage;

                if (ex is HttpRequestException)
                {
                    userMessage = "Unable to load quizzes due to a network issue. Please check your internet connection and try again.";
                }
                else
                {
                    userMessage = "An unexpected error occurred while loading quizzes. Please try again later.";
                }

                await Shell.Current.DisplayAlert("Oops...", userMessage, "OK");
            }

            IsBusy = false;
            IsRefreshing = false;
            _timer.Start();
        });
        return true;
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
        await LoadData();
    }

    [RelayCommand]
    private async Task OpenQuiz(int quizId)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await Shell.Current.DisplayAlert("Device Offline", "You must be online to start a quiz.", "OK");
            return;
        }

        var page = ActivatorUtilities.CreateInstance<QuizDetailsPage>(_provider, quizId);
        await Shell.Current.Navigation.PushAsync(page);
    }

    private bool CanOpenQuiz(int quizId)
    {
        var quiz = Quizzes.Collection.FirstOrDefault(q => q.Id == quizId);
        return quiz?.Passed == false;
    }

    public async void Receive(QuizzesUpdatedMessage message)
    {
        await LoadData();
    }

    public void OnDisappearing()
    {
        _timer.Stop();
        _timer.Tick -= OnScrollTick;
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

    public static bool IsEqual(QuizItemViewModel a, QuizItemViewModel b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        return a.Id == b.Id
            && a.Title == b.Title
            && a.Description == b.Description
            && a.Passed == b.Passed
            && a.ThumbnailImage == b.ThumbnailImage
            && a.CarouselImage == b.CarouselImage
            && a.IsCarousel == b.IsCarousel
            && a.Icon == b.Icon
            && a.Points == b.Points;
    }
}