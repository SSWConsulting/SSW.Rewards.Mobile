using SSW.Rewards.Mobile.Common;
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
    private readonly IApiErrorPresenter _errorPresenter;

    public QuizService(IApiQuizService quizService, IApiErrorPresenter errorPresenter)
    {
        _quizClient = quizService;
        _errorPresenter = errorPresenter;
    }

    public async Task<QuizDetailsDto> GetQuizDetails(int quizID)
    {
        try
        {
            return await _quizClient.GetQuizDetails(quizID, CancellationToken.None);
        }
        catch (Exception e)
        {
            await _errorPresenter.HandleLoadErrorAsync(e, "the quiz");

            return null;
        }
    }

    // Errors (offline, 401, ...) propagate: the Quiz page reports them, knowing whether cached quizzes
    // are still on screen. Returning nothing here would be cached over the real list.
    public async Task<IEnumerable<QuizDto>> GetQuizzes()
        => (await _quizClient.GetQuizzes(CancellationToken.None)).ToList();

    public async Task<QuizResultDto> SubmitQuiz(QuizSubmissionDto dto)
    {
        try
        {
            return await _quizClient.SubmitQuiz(dto, CancellationToken.None);
        }
        catch (Exception e)
        {
            await _errorPresenter.HandleActionErrorAsync(e, "submit your quiz");

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
            await _errorPresenter.HandleActionErrorAsync(e, "start the quiz");
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
            await _errorPresenter.HandleActionErrorAsync(e, "submit your answer");
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
            await _errorPresenter.HandleLoadErrorAsync(e, "the quiz results");

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
            await _errorPresenter.HandleLoadErrorAsync(e, "the quiz results");

            return null;
        }
    }
}
