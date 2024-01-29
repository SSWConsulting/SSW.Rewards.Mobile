using System.Net.Http.Headers;
using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.ApiClient.Services;

public interface IQuizAdminService
{
    Task<IEnumerable<QuizDetailsDto>> GetAdminQuizList(CancellationToken cancellationToken);

    Task<QuizEditDto> GetAdminQuizEdit(int quizId, CancellationToken cancellationToken);
    
    Task<string> UploadQuizImage(Stream file, string fileName, CancellationToken cancellationToken);

    Task<int> AddNewQuiz(QuizEditDto quizDetailsDto, CancellationToken cancellationToken);

    Task<int> UpdateQuiz(QuizEditDto quizDetailsDto, CancellationToken cancellationToken);
}

public class QuizAdminService : IQuizAdminService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Quizzes/";

    public QuizAdminService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<int> AddNewQuiz(QuizEditDto quizDetailsDto, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}AddNewQuiz", quizDetailsDto, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);

            return response;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to add new quiz: {responseContent}");
    }
    
    public async Task<int> UpdateQuiz(QuizEditDto quizDetailsDto, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PutAsJsonAsync($"{_baseRoute}UpdateQuiz", quizDetailsDto, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);

            return response;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to update quiz: {responseContent}");
    }
    
    public async Task<string> UploadQuizImage(Stream file, string fileName, CancellationToken cancellationToken)
    {
        var content = Helpers.ProcessImageContent(file, fileName);
        
        var result = await _httpClient.PostAsync($"{_baseRoute}UploadQuizImage", content, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadAsStringAsync(cancellationToken: cancellationToken);

            if (!string.IsNullOrEmpty(response))
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to upload carousel picture: {responseContent}");
    }

    public async Task<IEnumerable<QuizDetailsDto>> GetAdminQuizList(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetAdminQuizList", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<IEnumerable<QuizDetailsDto>>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get quizzes: {responseContent}");
    }

    public async Task<QuizEditDto> GetAdminQuizEdit(int quizId, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetAdminQuizEdit/{quizId}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<QuizEditDto>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get quiz: {responseContent}");
    }
}