using SSW.Rewards.Controls;
using SSW.Rewards.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class QuizDetailsViewModel : BaseViewModel
    {
        private readonly IQuizService _quizService;
        private int _quizId;

        public ObservableCollection<QuizQuestionViewModel> Questions { get; set; } = new ObservableCollection<QuizQuestionViewModel>();

        public ObservableCollection<QuestionResultDto> Results { get; set; } = new ObservableCollection<QuestionResultDto>();

        public ICommand ButtonCommand { get; set; }

        public ICommand BackCommand => new Command(async () => await GoBack());

        public ICommand CurrentQuestionChangedCommand => new Command(() => CurrentQuestionChanged());

        public ICommand ResultsButtonCommand { get; set; }

        public string QuizTitle { get; set; }

        public string QuizDescription { get; set; }

        public string ButtonText { get; set; } = "Next";

        public string Score { get; set; }

        public string ResultsTitle { get; set; }

        public string ResultButtonText { get; set; }

        public bool QuestionsVisible { get; set; } = true;

        public bool ResultsVisible { get; set; } = false;

        public bool TestPassed { get; set; }

        public SnackbarOptions SnackOptions { get; set; }

        public EventHandler<int> OnNextQuestionRequested;

        public EventHandler<ShowSnackbarEventArgs> ShowSnackbar;

        public QuizQuestionViewModel CurrentQuestion { get; set; }

        private string _quizIcon;

        public QuizDetailsViewModel(IQuizService quizService)
        {
            _quizService = quizService;
        }

        public async Task Initialise(int quizId, string icon)
        {
            _quizId = quizId;

            _quizIcon = icon;

            IsBusy = true;
            OnPropertyChanged(nameof(IsBusy));

            var quiz = await _quizService.GetQuizDetails(_quizId);

            Questions.Clear();

            foreach (var question in quiz.Questions.OrderBy(q => q.QuestionId))
            {
                Questions.Add(new QuizQuestionViewModel(question));
            }

            QuizTitle = quiz.Title;
            QuizDescription = quiz.Description;

            IsBusy = false;

            RaisePropertyChanged(nameof(IsBusy), nameof(QuizTitle), nameof(QuizDescription));
        }

        private async Task SubmitResponses()
        {
            bool allQuestionsAnswered = true;

            var answers = new List<QuizAnswer>();

            foreach (var question in Questions)
            {
                var answer = question.MyAnswers.FirstOrDefault(a => a.IsSelected);

                if (answer is null)
                {
                    allQuestionsAnswered = false;
                }
                else
                {
                    answers.Add(new QuizAnswer
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
                OnPropertyChanged(nameof(IsBusy));

                var result = await _quizService.SubmitQuiz(command);

                IsBusy = false;
                OnPropertyChanged(nameof(IsBusy));

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

                var args = new ShowSnackbarEventArgs { Options = SnackOptions };

                ShowSnackbar.Invoke(this, args);

                MessagingCenter.Send<object>(this, Constants.PointsAwardedMessage);
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
                    RaisePropertyChanged(nameof(QuestionsVisible), nameof(ResultsVisible));
                });

                Results.Clear();

                RaisePropertyChanged(nameof(QuestionsVisible), nameof(ResultsVisible), nameof(Score), nameof(ResultButtonText), nameof(ResultsTitle), nameof(TestPassed), nameof(ResultsButtonCommand));

                foreach (var questionResult in result.Results.OrderBy(r => r.QuestionId))
                {
                    Results.Add(questionResult);
                }
            }

            RaisePropertyChanged(nameof(QuestionsVisible), nameof(ResultsVisible), nameof(Score), nameof(ResultButtonText), nameof(ResultsTitle), nameof(TestPassed), nameof(ResultsButtonCommand));
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
            var selectedIndex = Questions.IndexOf(CurrentQuestion);

            var isLastQuestion = selectedIndex == Questions.Count - 1;

            if (isLastQuestion)
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
            RaisePropertyChanged(nameof(QuizDescription), nameof(ButtonText), nameof(ButtonCommand));
        }

        private void MoveNext(int next)
        {
            OnNextQuestionRequested.Invoke(this, next);
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

            foreach (var answer in questionDto.Answers)
            {
                MyAnswers.Add(new QuizAnswerViewModel
                {
                    QuestionAnswerId = answer.QuestionAnswerId,
                    Text = answer.Text,
                    QuestionId = questionDto.QuestionId
                });
            }
        }

        public ObservableCollection<QuizAnswerViewModel> MyAnswers { get; set; } = new ObservableCollection<QuizAnswerViewModel>();
    }
}
