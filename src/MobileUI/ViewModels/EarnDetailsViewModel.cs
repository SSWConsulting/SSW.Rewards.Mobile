using SSW.Rewards.Mobile.Controls;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Mobile.PopupPages;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Mobile.ViewModels
{
    public partial class EarnDetailsViewModel : BaseViewModel
    {
        private readonly IQuizService _quizService;
        private readonly ISnackbarService _snackbarService;
        private int _quizId;
        private string _quizIcon;
        private int _submissionId;
        private List<string> _loadingPhrases = new ()
        {
            "Summoning answers from our quiz master!",
            "Extracting brilliance from our genius AI companion!",
            "Harvesting insights from the all-knowing AI guru!",
            "Retrieving wisdom from the depths of the AI genius!",
            "Snatching enlightenment from our brainy AI overlord!"
        };
        
        [ObservableProperty]
        private string _animRef = "Sophie.json";
        
        [ObservableProperty]
        private string _loadingText = "Loading...";

        public ObservableCollection<EarnQuestionViewModel> Questions { get; } = [];

        public ObservableCollection<QuestionResultDto> Results { get; set; } = [];

        public ICommand BackCommand => new Command(async () => await GoBack());

        public ICommand MoveBackCommand => new Command(() => MoveNext(Questions.IndexOf(CurrentQuestion) - 1));

        public ICommand MoveNextCommand => new Command(async () => await SubmitAnswer());

        public ICommand SubmitCommand => new Command(async () => await SubmitResponses());

        public ICommand RecordAudioCommand => new Command(async () => await RecordAudio());

        public ICommand ResultsButtonCommand { get; set; }

        [ObservableProperty]
        private string _quizTitle;

        [ObservableProperty]
        private string _quizDescription;

        [ObservableProperty]
        private string _score;

        [ObservableProperty]
        private string _resultsTitle;

        [ObservableProperty]
        private string _resultButtonText;

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

        public EarnDetailsViewModel(IQuizService quizService, ISnackbarService snackbarService)
        {
            _quizService = quizService;
            _snackbarService = snackbarService;
        }

        public async Task Initialise(int quizId, string icon)
        {
            IsBusy = true;
            _quizId = quizId;
            _quizIcon = icon;
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

            MoveNext(Questions.IndexOf(CurrentQuestion) + 1);
            IsBusy = false;
        }

        private async Task SubmitResponses()
        {
            if (!CurrentQuestion.IsSubmitted)
            {
                await SubmitAnswer();
            }
            
            bool allQuestionsAnswered = Questions.All(q => q.IsSubmitted);

            if (allQuestionsAnswered)
            {
                var popup = new QuizResultPending();
                await MopupService.Instance.PushAsync(popup);

                var isComplete = await AwaitQuizCompletion();

                if (!isComplete)
                {
                    await App.Current.MainPage.DisplayAlert("Pending Results", $"Your quiz results are still being processed. Please try again.", "OK");
                    IsBusy = false;
                    await MopupService.Instance.RemovePageAsync(popup);
                    return;
                }

                var result = await _quizService.GetQuizResults(_submissionId);

                await ProcessResult(result);
                await MopupService.Instance.RemovePageAsync(popup);
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Incomplete Quiz", $"Some questions have not been answered. Please answer all questions to submit the quiz.", "OK");
            }
        }

        private async Task<bool> AwaitQuizCompletion()
        {
            var maxAttempts = 30;
            var delay = 5000;
            bool isComplete = false;

            while (!isComplete)
            {
                maxAttempts--;
                var completion = await _quizService.CheckQuizCompletion(_submissionId);

                if (completion == null || maxAttempts == 0)
                {
                    return false;
                }

                isComplete = completion.Value;
                await Task.Delay(delay);
            }

            return true;
        }

        public async Task ProcessResult(QuizResultDto result)
        {
            QuestionsVisible = false;
            ResultsVisible = true;

            var total = result.Results.Count();

            var correct = result.Results.Count(r => r.Correct);

            Score = $"{correct}/{total}";

            Results.Clear();

            foreach (var questionResult in result.Results.OrderBy(r => r.QuestionId))
            {
                Results.Add(questionResult);
            }

            if (result.Passed)
            {
                ResultButtonText = "Take Another Quiz";

                ResultsTitle = "Test Passed!";

                TestPassed = true;

                ResultsButtonCommand = new Command(async () => await GoBack());

                SnackOptions = new SnackbarOptions
                {
                    ActionCompleted = true,
                    GlyphIsBrand = true,
                    Glyph = _quizIcon,
                    Message = $"You have completed the {QuizTitle} quiz",
                    Points = result.Points,
                    ShowPoints = true
                };

                await _snackbarService.ShowSnackbar(SnackOptions);

                WeakReferenceMessenger.Default.Send(new PointsAwardedMessage());
            }
            else
            {
                ResultButtonText = "Try Again";

                ResultsTitle = "Test Failed";

                TestPassed = false;

                ResultsButtonCommand = new Command(() =>
                {
                    QuestionsVisible = true;
                    ResultsVisible = false;
                });
            }

            OnPropertyChanged(nameof(ResultsButtonCommand));
        }

        public async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
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

        private void MoveNext(int next)
        {
            if (next < 0 || next >= Questions.Count)
                return;

            CurrentQuestion = Questions[next];
            CurrentQuestionChanged();
        }

        private bool _isRecording;
        private CancellationTokenSource _audioRecordingCancellationTokenSource;
        private async Task RecordAudio()
        {
            if (_isRecording)
            {
                await _audioRecordingCancellationTokenSource.CancelAsync();
                _isRecording = false;
                return;
            }

            _isRecording = true;
            _audioRecordingCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _audioRecordingCancellationTokenSource.Token;
            
            var isGranted = await SpeechToText.Default.RequestPermissions(cancellationToken);
            if (!isGranted)
            {
                await Toast.Make("Permission not granted").Show(CancellationToken.None);
                return;
            }

            var recognitionResult = await SpeechToText.Default.ListenAsync(
                CultureInfo.GetCultureInfo("en-AU"),
                new Progress<string>(partialText =>
                {
                    CurrentQuestion.Answer += partialText + " ";
                }), cancellationToken);
            if (recognitionResult.IsSuccessful)
            {
                CurrentQuestion.Answer = recognitionResult.Text;
            }
            else
            {
                await Toast.Make(recognitionResult.Exception?.Message ?? "Unable to recognize speech").Show(CancellationToken.None);
            }
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