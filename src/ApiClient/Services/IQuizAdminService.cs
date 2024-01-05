using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.ApiClient.Services;

public interface IQuizAdminService
{
    Task<int> AddNewQuiz(QuizDetailsDto quizDetailsDto, CancellationToken cancellationToken);

    Task<int> UpdateQuiz(QuizDetailsDto quizDetailsDto, CancellationToken cancellationToken);
}

public class QuizAdminService : IQuizAdminService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Quizzes/";

    public QuizAdminService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int> AddNewQuiz(QuizDetailsDto quizDetailsDto, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}", quizDetailsDto, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);

            return response;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to add new quiz: {responseContent}");
    }

    public async Task<int> UpdateQuiz(QuizDetailsDto quizDetailsDto, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PutAsJsonAsync($"{_baseRoute}", quizDetailsDto, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);

            return response;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to update quiz: {responseContent}");
    }
}