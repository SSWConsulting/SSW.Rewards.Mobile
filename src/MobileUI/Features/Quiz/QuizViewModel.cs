using System.Text.Json.Serialization;
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
    private readonly IApiErrorPresenter _errorPresenter;

    private const int AutoScrollInterval = 6;
    private const string CacheKey = "QuizzesList";

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private int _carouselPosition;

    public AdvancedObservableCollection<QuizItemViewModel> Quizzes { get; } = new();
    public ObservableRangeCollection<QuizItemViewModel> CarouselQuizzes { get; set; } = [];

    public QuizViewModel(IQuizService quizService, IServiceProvider provider, IFileCacheService fileCacheService, IApiErrorPresenter errorPresenter)
    {
        _quizService = quizService;
        _provider = provider;
        _fileCacheService = fileCacheService;
        _errorPresenter = errorPresenter;
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
            // Quiet behind cached quizzes, except a 401, which still goes to re-login.
            await _errorPresenter.HandleLoadAsync(ListLoadResult.Fail(ex, Quizzes.IsLoaded), userRequestedNewData: false, "quizzes");

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
        if (!await _errorPresenter.EnsureOnlineAsync("start a quiz"))
        {
            return;
        }

        var page = ActivatorUtilities.CreateInstance<QuizDetailsPage>(_provider, quizId);
        await Shell.Current.Navigation.PushAsync(page);
    }

    public async void Receive(QuizzesUpdatedMessage message)
    {
        await LoadData();
    }

    public void OnDisappearing()
    {
        _timer.Stop();
        _timer.Tick -= OnScrollTick;
        Quizzes.OnCollectionUpdated -= OnQuizzesUpdated;
        Quizzes.OnError -= OnQuizzesError;
    }
}

[ObservableObject]
public partial class QuizItemViewModel : QuizDto
{
    [ObservableProperty] private bool _passed;

    [JsonConstructor]
    public QuizItemViewModel()
    {
        _passed = base.Passed;
    }

    public QuizItemViewModel(QuizDto questionDto) : this()
        => CopyFromDto(questionDto);

    private void CopyFromDto(QuizDto questionDto)
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