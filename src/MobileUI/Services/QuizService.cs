using SSW.Rewards.Shared.DTOs.Quizzes;
using IApiQuizService = SSW.Rewards.ApiClient.Services.IQuizService;

namespace SSW.Rewards.Mobile.Services;

public interface IQuizService
{
    Task<IEnumerable<QuizDto>> GetQuizzes();

    Task<QuizDetailsDto> GetQuizDetails(int quizID);

    Task<QuizResultDto> SubmitQuiz(QuizSubmissionDto dto);

    Task<BeginQuizDto> BeginQuiz(int quizId);

    Task SubmitAnswer(SubmitQuizAnswerDto dto);

    Task<bool?> CheckQuizCompletion(int submissionId);

    Task<QuizResultDto> GetQuizResults(int submissionId);
}

public class QuizService : IQuizService
{
    private readonly IApiQuizService _quizClient;

    public QuizService(IApiQuizService quizService)
    {
        _quizClient = quizService;
    }

    public async Task<QuizDetailsDto> GetQuizDetails(int quizID)
    {
        try
        {
            return await _quizClient.GetQuizDetails(quizID, CancellationToken.None);
        }
        catch (Exception e)
        {
            if (! await ExceptionHandler.HandleApiException(e))
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

            var apiQuizzes = await _quizClient.GetQuizzes(CancellationToken.None);

            foreach(var quiz in apiQuizzes)
            {
                quizzes.Add(quiz);
            }

            return quizzes;
        }
        catch (Exception e)
        {
            if (! await ExceptionHandler.HandleApiException(e))
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem loading the quizzes. Please try again soon.", "OK");
            }

            return null;
        }
    }

    public async Task<QuizResultDto> SubmitQuiz(QuizSubmissionDto dto)
    {
        try
        {
            return await _quizClient.SubmitQuiz(dto, CancellationToken.None);
        }
        catch (Exception e)
        {
            if (! await ExceptionHandler.HandleApiException(e))
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem submitting your quiz. Please try again soon.", "OK");
            }

            return null;
        }
    }
    
    public async Task<BeginQuizDto> BeginQuiz(int quizId)
    {
        try
        {
            return await _quizClient.BeginQuiz(quizId, CancellationToken.None);
        }
        catch (Exception e)
        {
            if (! await ExceptionHandler.HandleApiException(e))
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem beginning the quiz. Please try again soon.", "OK");
            }
        }

        return null;
    }
    
    public async Task SubmitAnswer(SubmitQuizAnswerDto dto)
    {
        try
        {
            await _quizClient.SubmitAnswer(dto, CancellationToken.None);
        }
        catch (Exception e)
        {
            if (! await ExceptionHandler.HandleApiException(e))
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem submitting your answer. Please try again soon.", "OK");
            }
        }
    }
    
    public async Task<bool?> CheckQuizCompletion(int submissionId)
    {
        try
        {
            return await _quizClient.CheckQuizCompletion(submissionId, CancellationToken.None);
        }
        catch (Exception e)
        {
            if (! await ExceptionHandler.HandleApiException(e))
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem loading the quiz details. Please try again soon.", "OK");
            }

            return null;
        }
    }
    
    public async Task<QuizResultDto> GetQuizResults(int submissionId)
    {
        try
        {
            return await _quizClient.GetQuizResults(submissionId, CancellationToken.None);
        }
        catch (Exception e)
        {
            if (! await ExceptionHandler.HandleApiException(e))
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem loading the quiz results. Please try again soon.", "OK");
            }

            return null;
        }
    }
}
