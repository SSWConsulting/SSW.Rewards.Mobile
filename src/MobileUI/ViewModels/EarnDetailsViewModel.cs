using SSW.Rewards.Mobile.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Mobile.ViewModels
{
    public partial class EarnDetailsViewModel : BaseViewModel
    {
        private readonly IQuizService _quizService;
        private readonly ISnackbarService _snackbarService;
        private int _quizId;
        private int _submissionId;

        public ObservableCollection<EarnQuestionViewModel> Questions { get; set; } = new ObservableCollection<EarnQuestionViewModel>();

        public ObservableCollection<QuestionResultDto> Results { get; set; } = new ObservableCollection<QuestionResultDto>();

        public ICommand BackCommand => new Command(async () => await GoBack());
        
        public ICommand MoveBackCommand => new Command(() => MoveNext(Questions.IndexOf(CurrentQuestion) - 1));
        
        public ICommand MoveNextCommand => new Command(async () => await SubmitAnswer());

        public ICommand SubmitCommand => new Command(async () => await SubmitResponses());

        public ICommand CurrentQuestionChangedCommand => new Command(() => CurrentQuestionChanged());

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

        public SnackbarOptions SnackOptions { get; set; }

        public EventHandler<int> OnNextQuestionRequested;

        public EarnQuestionViewModel CurrentQuestion { get; set; }

        private string _quizIcon;

        private bool IsLoadingQuestions { get; set; } = false;

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

            IsLoadingQuestions = false;
            QuizTitle = quiz.Title;
            ThumbnailImage = quiz.ThumbnailImage;
            QuizDescription = quiz.Description;
            Points = quiz.Points;

            IsBusy = false;
        }

        private async Task SubmitAnswer()
        {
            var question = Questions.FirstOrDefault(q => q.QuestionId == CurrentQuestion.QuestionId);

            if (!string.IsNullOrEmpty(CurrentQuestion.Answer))
            {
                await _quizService.SubmitAnswer(new SubmitQuizAnswerDto()
                {
                    AnswerText = CurrentQuestion.Answer, QuestionId = CurrentQuestion.QuestionId, SubmissionId = _submissionId
                });

                if (question != null)
                {
                    question.IsSubmitted = true;
                }
            }
            
            MoveNext(Questions.IndexOf(CurrentQuestion) + 1);
        }

        private async Task SubmitResponses()
        {
            await SubmitAnswer();
            bool allQuestionsAnswered = Questions.All(q => q.IsSubmitted);
            
            if (allQuestionsAnswered)
            {
                IsBusy = true;

                var isComplete = await AwaitQuizCompletion();

                if (!isComplete)
                {
                    await App.Current.MainPage.DisplayAlert("Pending Results", $"Your quiz results are still being processed. Please try again.", "OK");
                    IsBusy = false;
                    return;
                }

                var result = await _quizService.GetQuizResults(_submissionId);

                IsBusy = false;

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

                ResultsButtonCommand = new Command(async () => await GoBack(false));

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

        private async Task GoBack(bool askFirst = true)
        {
            bool confirmed;
            
            if (askFirst)
            {
                confirmed = await App.Current.MainPage.DisplayAlert("Leave Quiz", "Are you sure you want to quit this quiz?", "Yes", "No");
            }
            else
            {
                confirmed = true;
            }
            

            if (confirmed)
                await Shell.Current.GoToAsync("..");
        }

        private void CurrentQuestionChanged()
        {
            if (CurrentQuestion == null)
                return;

            var selectedIndex = Questions.IndexOf(CurrentQuestion);
            var isLastQuestion = selectedIndex == Questions.Count - 1;
            
            IsFirstQuestion = selectedIndex == 0;
            IsLastQuestion = isLastQuestion && !IsLoadingQuestions;
        }

        private void MoveNext(int next)
        {
            OnNextQuestionRequested.Invoke(this, next);
        }

        public void Clear()
        {
            Questions.Clear();
            Results.Clear();
            QuizTitle = "";
            QuizDescription = "";
            ThumbnailImage = "";
            Points = 0;
            QuestionsVisible = true;
            ResultsVisible = false;
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