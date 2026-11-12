using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class QuizViewModel : BaseViewModel, IRecipient<QuizzesUpdatedMessage>, IRefreshOnReconnect
{
    private const string QuizzesSubject = "quizzes";

    private readonly IServiceProvider _provider;
    private readonly IDispatcherTimer _timer;
    private readonly IApiErrorPresenter _errorPresenter;

    private const int AutoScrollInterval = 6;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private int _carouselPosition;

    public PagedListSource<QuizItemViewModel> Quizzes { get; }
    public ObservableRangeCollection<QuizItemViewModel> CarouselQuizzes { get; set; } = [];

    public QuizViewModel(IQuizService quizService, IServiceProvider provider, IFileCacheService fileCacheService, IApiErrorPresenter errorPresenter)
    {
        _provider = provider;
        _errorPresenter = errorPresenter;
        WeakReferenceMessenger.Default.Register(this);
        _timer = Application.Current.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(AutoScrollInterval);

        Quizzes = new PagedListSource<QuizItemViewModel>(new()
        {
            FetchPage = async (_, _, _) =>
                (await quizService.GetQuizzes()).Select(quiz => new QuizItemViewModel(quiz)).ToList(),
            PageSize = PagedListSourceOptions<QuizItemViewModel>.SinglePage,
            AreSame = QuizItemViewModel.IsEqual,
            Cache = fileCacheService,
            CacheKey = "QuizzesList",
        });

        // Cached quizzes land before the network: show the carousel and drop the spinner then.
        Quizzes.Items.CollectionChanged += (_, _) => OnQuizzesShown();
    }

    public async Task Initialise()
    {
        if (!Quizzes.HasContent)
        {
            await LoadData();
        }

        BeginAutoScroll();
    }

    private async Task LoadData()
    {
        IsBusy = !Quizzes.HasContent;
        _timer.Stop();
        CarouselPosition = 0;

        try
        {
            var result = await Quizzes.RefreshAsync();

            // Quiet behind cached quizzes, except a 401, which still goes to re-login.
            await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: false, QuizzesSubject);
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
            _timer.Start();
        }
    }

    private void OnQuizzesShown()
    {
        if (Quizzes.HasContent)
        {
            IsBusy = false;
        }

        var carousel = Quizzes.AllItems.Where(q => q.IsCarousel).ToList();
        if (!carousel.SequenceEqual(CarouselQuizzes))
        {
            CarouselQuizzes.ReplaceRange(carousel);
        }
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

    public Task RefreshOnReconnectAsync() => LoadData();

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