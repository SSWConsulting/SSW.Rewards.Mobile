using SSW.Rewards.Mobile.Controls;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Mobile.PopupPages;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Mobile.ViewModels
{
    public partial class QuizDetailsViewModel : BaseViewModel
    {
        private readonly IQuizService _quizService;
        private readonly ISnackbarService _snackbarService;
        private readonly IUserService _userService;
        private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
        private int _quizId;
        private int _submissionId;

        [ObservableProperty]
        private string _animRef = "Sophie.json";

        [ObservableProperty]
        private string _loadingText = "Loading...";

        public ObservableCollection<EarnQuestionViewModel> Questions { get; } = [];

        public ObservableCollection<QuestionResultDto> Results { get; set; } = [];

        [ObservableProperty]
        private string _quizTitle;

        [ObservableProperty]
        private string _quizDescription;
        
        [ObservableProperty]
        private Color _scoreBackground;

        [ObservableProperty]
        private string _score;

        [ObservableProperty]
        private string _resultsTitle;

        [ObservableProperty]
        private bool _questionsVisible = true;

        [ObservableProperty]
        private bool _resultsVisible;

        [ObservableProperty]
        private bool _testPassed;

        [ObservableProperty]
        private string _icon;

        [ObservableProperty]
        private string _thumbnailImage;

        [ObservableProperty]
        private int _points;

        [ObservableProperty]
        private bool _isFirstQuestion = true;

        [ObservableProperty]
        private bool _isLastQuestion;

        [ObservableProperty]
        private bool _isSubmitted;

        [ObservableProperty]
        private EarnQuestionViewModel _currentQuestion;

        [ObservableProperty]
        private int _currentQuestionIndex;

        [ObservableProperty]
        private bool _isLoadingQuestions;

        public SnackbarOptions SnackOptions { get; set; }

        public QuizDetailsViewModel(IQuizService quizService, ISnackbarService snackbarService, IUserService userService, IFirebaseAnalyticsService firebaseAnalyticsService)
        {
            _quizService = quizService;
            _snackbarService = snackbarService;
            _userService = userService;
            _firebaseAnalyticsService = firebaseAnalyticsService;
        }

        public async Task Initialise(int quizId)
        {
            IsBusy = true;
            _quizId = quizId;
            IsLoadingQuestions = true;

            var quiz = await _quizService.GetQuizDetails(_quizId);
            var beginQuiz = await _quizService.BeginQuiz(_quizId);
            _submissionId = beginQuiz.SubmissionId;

            foreach (var question in quiz.Questions.OrderBy(q => q.QuestionId))
            {
                Questions.Add(new EarnQuestionViewModel(question));
            }
            CurrentQuestion = Questions.First();
            IsLoadingQuestions = false;
            QuizTitle = quiz.Title;
            ThumbnailImage = quiz.ThumbnailImage;
            QuizDescription = quiz.Description;
            Points = quiz.Points;

            IsBusy = false;

            LogEvent(Constants.AnalyticsEvents.QuizStart);
            
            WeakReferenceMessenger.Default.Send(new TopBarAvatarMessage(AvatarOptions.Back));
        }

        private void LogEvent(string eventName)
        {
            _firebaseAnalyticsService.Log(eventName,
                new Dictionary<string, string> { { "quiz_id", _quizId.ToString() }, { "quiz_title", QuizTitle } });
        }

        private async Task SubmitAnswer()
        {
            IsBusy = true;
            var question = Questions.FirstOrDefault(q => q.QuestionId == CurrentQuestion.QuestionId);

            if (!CurrentQuestion.IsSubmitted && !string.IsNullOrEmpty(CurrentQuestion.Answer))
            {
                await _quizService.SubmitAnswer(new SubmitQuizAnswerDto
                {
                    AnswerText = CurrentQuestion.Answer, QuestionId = CurrentQuestion.QuestionId, SubmissionId = _submissionId
                });

                if (question != null)
                {
                    question.IsSubmitted = true;
                }
            }

            MoveTo(Questions.IndexOf(CurrentQuestion) + 1);
            IsBusy = false;
        }

        [RelayCommand]
        private async Task SubmitResponses()
        {
            if (!CurrentQuestion.IsSubmitted)
            {
                await SubmitAnswer();
            }

            bool allQuestionsAnswered = Questions.All(q => q.IsSubmitted);

            if (allQuestionsAnswered)
            {
                var popup = new QuizResultPendingPage(new QuizResultPendingViewModel());
                await MopupService.Instance.PushAsync(popup);

                var isComplete = await AwaitQuizCompletion();

                if (!isComplete)
                {
                    IsBusy = false;
                    await MopupService.Instance.RemovePageAsync(popup);
                    await App.Current.MainPage.DisplayAlert("Pending Results", $"Your quiz results are still being processed. Please try again soon.", "OK");
                    return;
                }

                var result = await _quizService.GetQuizResults(_submissionId);

                await MopupService.Instance.RemovePageAsync(popup);
                await ProcessResult(result);
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Incomplete Quiz", $"Some questions have not been answered. Please answer all questions to submit the quiz.", "OK");
            }
        }

        private async Task<bool> AwaitQuizCompletion()
        {
            var maxAttempts = 30;
            var delay = 2000;
            bool isComplete = false;

            while (!isComplete)
            {
                maxAttempts--;
                var completion = await _quizService.CheckQuizCompletion(_submissionId);

                if (completion == null || (completion == false && maxAttempts == 0))
                {
                    return false;
                }

                isComplete = completion.Value;
                await Task.Delay(delay);
            }

            return true;
        }

        private async Task ProcessResult(QuizResultDto result)
        {
            QuestionsVisible = false;
            ResultsVisible = true;
            WeakReferenceMessenger.Default.Send(new TopBarAvatarMessage(AvatarOptions.Done));

            var total = result.Results.Count;
            var correct = result.Results.Count(r => r.Correct);
            int index = 1;

            Score = $"{correct}/{total}";

            Results.Clear();

            foreach (var questionResult in result.Results.OrderBy(r => r.QuestionId))
            {
                questionResult.Index = index++;
                Results.Add(questionResult);
            }

            if (result.Passed)
            {
                LogEvent(Constants.AnalyticsEvents.QuizPass);
                App.Current.Resources.TryGetValue("SuccessGreen", out object successGreen);

                ScoreBackground = (Color)successGreen!;
                ResultsTitle = "Good job!";
                TestPassed = true;
                SnackOptions = new SnackbarOptions
                {
                    ActionCompleted = true,
                    GlyphIsBrand = false,
                    Glyph = "\uf837", // Trophy icon
                    Message = $"You have completed the {QuizTitle}",
                    Points = Points,
                    ShowPoints = true
                };

                await _userService.UpdateMyDetailsAsync();
                await _snackbarService.ShowSnackbar(SnackOptions);
            }
            else
            {
                LogEvent(Constants.AnalyticsEvents.QuizFail);
                App.Current.Resources.TryGetValue("SSWRed", out object sswRed);

                ScoreBackground = (Color)sswRed!;
                ResultsTitle = "Don't give up!";
                TestPassed = false;
            }
        }

        private void CurrentQuestionChanged()
        {
            if (CurrentQuestion == null)
                return;

            CurrentQuestionIndex = Questions.IndexOf(CurrentQuestion);
            var isLastQuestion = CurrentQuestionIndex == Questions.Count - 1;

            IsFirstQuestion = CurrentQuestionIndex == 0;
            IsLastQuestion = isLastQuestion && !IsLoadingQuestions;
        }

        private void MoveTo(int index)
        {
            if (index < 0 || index >= Questions.Count)
                return;

            CurrentQuestion = Questions[index];
            CurrentQuestionChanged();
        }

        [RelayCommand]
        private void MoveBack()
        {
            MoveTo(Questions.IndexOf(CurrentQuestion) - 1);
        }

        [RelayCommand]
        private async Task MoveForward()
        {
            await SubmitAnswer();
        }

        [RelayCommand]
        private void AnswerChanged(TextChangedEventArgs args)
        {
            CurrentQuestion.Answer = args.NewTextValue;
        }
    }

    [ObservableObject]
    public partial class EarnQuestionViewModel : QuizQuestionDto
    {
        [ObservableProperty] private bool _isSubmitted;

        [ObservableProperty] private string _answer;

        public EarnQuestionViewModel(QuizQuestionDto questionDto)
        {
            this.QuestionId = questionDto.QuestionId;
            this.Text = questionDto.Text;
            this.Answer = questionDto.Answer;
        }
    }
}