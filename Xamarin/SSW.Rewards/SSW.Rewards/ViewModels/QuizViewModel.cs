using SSW.Rewards.Helpers;
using SSW.Rewards.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class QuizViewModel : BaseViewModel
    {
        public const string QuizzesUpdatedMessage = "quizzesupdated";

        private readonly IQuizService _quizService;

        private string quizDetailsPageUrl = "quiz/details";

        public ObservableCollection<QuizDto> Quizzes { get; set; } = new ObservableCollection<QuizDto>();

        public ICommand OpenQuizCommand { get; set; }

        public QuizViewModel(IQuizService quizService)
        {
            _quizService = quizService;

            OpenQuizCommand = new Command<int>(
                execute:
                async (id) => 
                {
                    var quiz = Quizzes.FirstOrDefault(q => q.Id == id);
                    await OpenQuiz(id, quiz.Icon);
                },
                canExecute:
                (id) =>
                {
                    return Quizzes.FirstOrDefault(q => q.Id == id).Passed == false;
                });

            MessagingCenter.Subscribe<object>(this, QuizzesUpdatedMessage, async (obj) => await Initialise());
        }

        public async Task Initialise()
        {
            Quizzes.Clear();

            IsBusy = true;

            OnPropertyChanged(nameof(IsBusy));

            var quizzes = await _quizService.GetQuizzes();

            foreach (var quiz in quizzes)
            {
                Quizzes.Add(quiz);
            }

            IsBusy = false;

            OnPropertyChanged(nameof(IsBusy));
        }

        private async Task OpenQuiz(int quizId, Icons icon)
        {
            await AppShell.Current.GoToAsync($"{quizDetailsPageUrl}?QuizId={quizId}&QuizIcon={icon}");
        }
    }
}
