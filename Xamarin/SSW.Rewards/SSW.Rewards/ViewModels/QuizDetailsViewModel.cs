using Newtonsoft.Json;
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

        public ICommand ButtonCommand { get; set; }

        public ICommand BackCommand => new Command(async () => await GoBack());

        public ICommand CurrentQuestionChangedCommand => new Command(() => CurrentQuestionChanged());

        public string QuizTitle { get; set; }

        public string QuizDescription { get; set; }

        public string ButtonText { get; set; } = "Next";

        public EventHandler<int> OnNextQuestionRequested;

        public QuizQuestionViewModel CurrentQuestion { get; set; }

        public QuizDetailsViewModel(IQuizService quizService)
        {
            _quizService = quizService;
        }

        public async Task Initialise(int quizId)
        {
            _quizId = quizId;

            IsBusy = true;
            OnPropertyChanged(nameof(IsBusy));

            var quiz = await _quizService.GetQuizDetails(_quizId);

            foreach (var question in quiz.Questions)
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

                var response = JsonConvert.SerializeObject(command);

                IsBusy = true;
                OnPropertyChanged(nameof(IsBusy));

                var result = await _quizService.SubmitQuiz(command);

                IsBusy = false;
                OnPropertyChanged(nameof(IsBusy));

                if (result.Passed)
                {
                    // do congrats and stuff
                    await App.Current.MainPage.DisplayAlert("Congratulations!", $"You passed the {QuizTitle} quiz and earned {result.Points} points.", "OK");

                    MessagingCenter.Send<object>(this, QuizViewModel.QuizzesUpdatedMessage);
                }
                else
                {
                    // do commiserations and stuff
                    await App.Current.MainPage.DisplayAlert("You Died.", String.Empty, "OK");
                }
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Incomplete Quiz", $"Some questions have not been answered. Please answer all questions to submit the quiz. {Environment.NewLine}TIP: You can swipe backwards through the questions.", "OK");
            }
        }

        private async Task GoBack()
        {
            var confirmed = await App.Current.MainPage.DisplayAlert("Leave Quiz", "Are you sure you want to quit this quiz?", "Yes", "No");

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
