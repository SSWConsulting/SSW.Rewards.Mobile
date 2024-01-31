using System.Net;
using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.ApiClient.Services;

public interface IQuizService
{
    Task<IEnumerable<QuizDto>> GetQuizzes(CancellationToken cancellationToken);

    Task<QuizDetailsDto> GetQuizDetails(int quizID, CancellationToken cancellationToken);

    Task<QuizResultDto> SubmitQuiz(QuizSubmissionDto submission, CancellationToken cancellationToken);

    Task<BeginQuizDto> BeginQuiz(int quizId, CancellationToken cancellationToken);

    Task SubmitAnswer(SubmitQuizAnswerDto answer, CancellationToken cancellationToken);

    Task<bool> CheckQuizCompletion(int submissionId, CancellationToken cancellationToken);

    Task<QuizResultDto> GetQuizResults(int submissionId, CancellationToken cancellationToken);
}

public class QuizService : IQuizService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Quizzes/";

    public QuizService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<IEnumerable<QuizDto>> GetQuizzes(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetQuizListForUser", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<IEnumerable<QuizDto>>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get quizzes: {responseContent}");
    }

    public async Task<QuizDetailsDto> GetQuizDetails(int quizID, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetQuizDetails/{quizID}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<QuizDetailsDto>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get quiz details: {responseContent}");
    }

    public async Task<QuizResultDto> SubmitQuiz(QuizSubmissionDto submission, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}SubmitCompletedQuiz", submission, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<QuizResultDto>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to submit quiz: {responseContent}");
    }
    
    public async Task<BeginQuizDto> BeginQuiz(int quizId, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}BeginQuiz", quizId, cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<BeginQuizDto>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to start quiz: {responseContent}");
    }
    
    public async Task SubmitAnswer(SubmitQuizAnswerDto answer, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}SubmitAnswer", answer, cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to submit answer: {responseContent}");
    }
    
    public async Task<bool> CheckQuizCompletion(int submissionId, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}CheckQuizCompletion/{submissionId}", cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            return result.StatusCode == HttpStatusCode.OK;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to check quiz completion: {responseContent}");
    }
    
    public async Task<QuizResultDto> GetQuizResults(int submissionId, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetQuizResults/{submissionId}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<QuizResultDto>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get quiz results: {responseContent}");
    }
}