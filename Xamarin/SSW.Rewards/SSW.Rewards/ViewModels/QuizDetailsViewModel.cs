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

        public ObservableCollection<QuizQuestionDto> Questions { get; set; } = new ObservableCollection<QuizQuestionDto>();

        private List<QuizAnswer> _answers = new List<QuizAnswer>();

        public ICommand ButtonCommand { get; set; }

        public ICommand BackCommand => new Command(async () => await GoBack());

        public ICommand CurrentQuestionChangedCommand => new Command(() => CurrentQuestionChanged());

        public string QuizTitle { get; set; }

        public string QuizDescription { get; set; }

        public string ButtonText { get; set; } = "Next";

        public EventHandler<int> OnNextQuestionRequested;

        public QuizQuestionDto CurrentQuestion { get; set; }

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
                Questions.Add(question);
            }

            QuizTitle = quiz.Title;
            QuizDescription = quiz.Description;

            IsBusy = false;

            RaisePropertyChanged(nameof(IsBusy), nameof(QuizTitle), nameof(QuizDescription));
        }

        public void AddOrUpdateAnswer(QuizAnswer answer)
        {
            var response = _answers.FirstOrDefault(a => a.QuestionId == answer.QuestionId);

            if (response is null)
            {
                _answers.Add(answer);
            }
            else
            {
                response.SelectedAnswerId = answer.SelectedAnswerId;
            }
        }

        private async Task SubmitResponses()
        {
            var command = new SubmitUserQuizCommand
            {
                Answers = _answers,
                QuizId = _quizId
            };

            var result = await _quizService.SubmitQuiz(command);

            if (result.Passed)
            {
                // do congrats and stuff

                MessagingCenter.Send<object>(this, QuizViewModel.QuizzesUpdatedMessage);
            }
            else
            {
                // do commiserations and stuff
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
        public bool IsSelected { get; set; }
    }
}
