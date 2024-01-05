using SSW.Rewards.Mobile.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.ViewModels
{
    public partial class QuizDetailsViewModel : BaseViewModel
    {
        private readonly IQuizService _quizService;
        private readonly ISnackbarService _snackbarService;
        private int _quizId;

        public ObservableCollection<QuizQuestionViewModel> Questions { get; set; } = new ObservableCollection<QuizQuestionViewModel>();

        public ObservableCollection<QuestionResultDto> Results { get; set; } = new ObservableCollection<QuestionResultDto>();

        public ICommand ButtonCommand { get; set; }

        public ICommand BackCommand => new Command(async () => await GoBack());

        public ICommand CurrentQuestionChangedCommand => new Command(() => CurrentQuestionChanged());

        public ICommand ResultsButtonCommand { get; set; }
        
        [ObservableProperty]
        private string _quizTitle;
        
        [ObservableProperty]
        private string _quizDescription;
        
        [ObservableProperty]
        private string _buttonText = "Next";
        
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

        public SnackbarOptions SnackOptions { get; set; }

        public EventHandler<int> OnNextQuestionRequested;

        public QuizQuestionViewModel CurrentQuestion { get; set; }

        private string _quizIcon;

        private bool IsLoadingQuestions { get; set; } = false;

        public QuizDetailsViewModel(IQuizService quizService, ISnackbarService snackbarService)
        {
            _quizService = quizService;
            _snackbarService = snackbarService;
        }

        public async Task Initialise(int quizId, string icon)
        {
            _quizId = quizId;

            _quizIcon = icon;

            IsLoadingQuestions = true;
            Clear();

            var quiz = await _quizService.GetQuizDetails(_quizId);

            foreach (var question in quiz.Questions.OrderBy(q => q.QuestionId))
            {
                Questions.Add(new QuizQuestionViewModel(question));
            }

            IsLoadingQuestions = false;
            QuizTitle = quiz.Title;

            IsBusy = false;
        }

        private async Task SubmitResponses()
        {
            bool allQuestionsAnswered = true;

            var answers = new List<QuizAnswerDto>();

            foreach (var question in Questions)
            {
                var answer = question.MyAnswers.FirstOrDefault(a => a.IsSelected);

                if (answer is null)
                {
                    allQuestionsAnswered = false;
                }
                else
                {
                    answers.Add(new QuizAnswerDto
                    {
                        SelectedAnswerId = answer.QuestionAnswerId,
                        QuestionId = answer.QuestionId
                    });
                }
            }

            if (allQuestionsAnswered)
            {
                var command = new SubmitUserQuizCommand
                {
                    Answers = answers,
                    QuizId = _quizId
                };

                IsBusy = true;

                var result = await _quizService.SubmitQuiz(command);

                IsBusy = false;

                await ProcessResult(result);
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Incomplete Quiz", $"Some questions have not been answered. Please answer all questions to submit the quiz. {Environment.NewLine}{Environment.NewLine}TIP: You can swipe backwards through the questions.", "OK");
            }
        }

        public async Task ProcessResult(QuizResultDto result)
        {
            QuestionsVisible = false;
            ResultsVisible = true;

            var total = result.Results.Count();

            var correct = result.Results.Count(r => r.Correct);

            Score = $"{correct}/{total}";

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

                Results.Clear();
                
                OnPropertyChanged(nameof(ResultsButtonCommand));
                
                foreach (var questionResult in result.Results.OrderBy(r => r.QuestionId))
                {
                    Results.Add(questionResult);
                }
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

            if (isLastQuestion && !IsLoadingQuestions)
            {
                ButtonText = "Submit";
                ButtonCommand = new Command(async () => await SubmitResponses());
            }
            else
            {
                ButtonText = "Next";
                ButtonCommand = new Command(() => MoveNext(++selectedIndex));
            }

            QuizDescription = CurrentQuestion.Text;
            OnPropertyChanged(nameof(ButtonCommand));
        }

        private void MoveNext(int next)
        {
            OnNextQuestionRequested.Invoke(this, next);
        }

        private void Clear()
        {
            Questions.Clear();
            Results.Clear();
            QuizTitle = "";
            QuizDescription = "";
            IsBusy = true;
            QuestionsVisible = true;
            ResultsVisible = false;
            RaisePropertyChanged(nameof(IsBusy), nameof(QuizTitle), nameof(QuizDescription), nameof(QuestionsVisible), nameof(ResultsVisible));
        }
    }

    public class QuizAnswerViewModel : QuestionAnswerDto
    {
        public int QuestionId { get; set; }

        public bool IsSelected { get; set; }
    }

    public class QuizQuestionViewModel : QuizQuestionDto
    {
        public QuizQuestionViewModel(QuizQuestionDto questionDto)
        {
            this.QuestionId = questionDto.QuestionId;
            this.Text = questionDto.Text;

            var counter = 0;
            foreach (var answer in questionDto.Answers)
            {
                var letter = (char)('A' + counter);
                MyAnswers.Add(new QuizAnswerViewModel
                {
                    QuestionAnswerId = answer.QuestionAnswerId,
                    Text = $"{letter}. {answer.Text}",
                    QuestionId = questionDto.QuestionId
                });
                counter++;
            }
        }

        public ObservableCollection<QuizAnswerViewModel> MyAnswers { get; set; } = new ObservableCollection<QuizAnswerViewModel>();
    }
}