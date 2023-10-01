namespace SSW.Rewards.Services;

public class QuizService : BaseService, IQuizService
{
    private QuizzesClient _quizClient;

    public QuizService(IHttpClientFactory clientFactory, ApiOptions options) : base(clientFactory, options)
    {
        _quizClient = new QuizzesClient(BaseUrl, AuthenticatedClient);
    }

    public async Task<QuizDetailsDto> GetQuizDetails(int quizID)
    {
        try
        {
            return await _quizClient.GetQuizDetailsAsync(quizID);
        }
        catch (ApiException e)
        {
            if (e.StatusCode == 401)
            {
                await App.Current.MainPage.DisplayAlert("Authentication Failure", "Looks like your session has expired. Choose OK to go back to the login screen.", "OK");
                await Application.Current.MainPage.Navigation.PushModalAsync<LoginPage>();
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem loading the quiz. Please try again soon.", "OK");
            }

            return null;
        }
    }

    public async Task<IEnumerable<QuizDto>> GetQuizzes()
    {
        try
        {
            var quizzes = new List<QuizDto>();

            var apiQuizzes = await _quizClient.GetQuizListForUserAsync();

            foreach(var quiz in apiQuizzes)
            {
                quizzes.Add(quiz);
            }

            return quizzes;
        }
        catch (ApiException e)
        {
            if (e.StatusCode == 401)
            {
                await App.Current.MainPage.DisplayAlert("Authentication Failure", "Looks like your session has expired. Choose OK to go back to the login screen.", "OK");
                await Application.Current.MainPage.Navigation.PushModalAsync<LoginPage>();
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem loading the quizzes. Please try again soon.", "OK");
            }

            return null;
        }
    }

    public async Task<QuizResultDto> SubmitQuiz(SubmitUserQuizCommand command)
    {
        try
        {
            return await _quizClient.SubmitCompletedQuizAsync(command);
        }
        catch (ApiException e)
        {
            if (e.StatusCode == 401)
            {
                await App.Current.MainPage.DisplayAlert("Authentication Failure", "Looks like your session has expired. Choose OK to go back to the login screen.", "OK");
                await Application.Current.MainPage.Navigation.PushModalAsync<LoginPage>();
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem submitting your quiz. Please try again soon.", "OK");
            }

            return null;
        }
    }
}
