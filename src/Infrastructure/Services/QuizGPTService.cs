using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand;
using SSW.Rewards.Infrastructure.Options;

namespace SSW.Rewards.Infrastructure.Services;

public sealed class QuizGPTService : IQuizGPTService
{
    private readonly HttpClient _httpClient;
    private readonly string _url;
    public QuizGPTService(IOptions<GPTServiceOptions> options, IHttpClientFactory httpFactory)
    {
        this._url        = options.Value.Url ?? throw new ArgumentNullException("No options for GPT Service found!");
        this._httpClient = httpFactory.CreateClient();
    }
    public async Task<QuizGPTResponseDto> ValidateAnswer(QuizGPTRequestDto model, CancellationToken ct)
    {
        var json        = JsonSerializer.Serialize(model);
        var data        = new StringContent(json, Encoding.UTF8, "application/json");
        var response    = await _httpClient.PostAsync(_url, data, ct);
        
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        QuizGPTResponseDto? result = JsonSerializer.Deserialize<QuizGPTResponseDto>(responseString);

        if (result == null)
            throw new Exception("Response from GPT API was null");
        
        return result;
    }
}
