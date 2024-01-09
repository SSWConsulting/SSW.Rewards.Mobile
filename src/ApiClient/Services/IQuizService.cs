using System.Net;
using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.ApiClient.Services;

public interface IQuizService
{
    Task<IEnumerable<QuizDto>> GetQuizzes(CancellationToken cancellationToken);

    Task<IEnumerable<QuizDetailsDto>> GetQuizDetails(CancellationToken cancellationToken);

    Task<QuizDetailsDto> GetQuizDetails(int quizID, CancellationToken cancellationToken);

    Task<QuizResultDto> SubmitQuiz(QuizSubmissionDto submission, CancellationToken cancellationToken);
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
        var result = await _httpClient.GetAsync($"{_baseRoute}", cancellationToken);

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

    public async Task<IEnumerable<QuizDetailsDto>> GetQuizDetails(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}Details", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<IEnumerable<QuizDetailsDto>>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException();
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get quiz details: {responseContent}");
    }

    public async Task<QuizDetailsDto> GetQuizDetails(int quizID, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}Details/{quizID}", cancellationToken);

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
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}Submit", submission, cancellationToken);

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
}