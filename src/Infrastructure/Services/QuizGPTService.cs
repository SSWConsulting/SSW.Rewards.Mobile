using System.Text;
using System.Text.Json;
using System.Threading;
using Hangfire;
using Microsoft.Extensions.Options;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Infrastructure.Options;

namespace SSW.Rewards.Infrastructure.Services;

public sealed class QuizGPTService : IQuizGPTService
{
    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IApplicationDbContext _context;
    public QuizGPTService(
        IOptions<GPTServiceOptions> options, 
        IHttpClientFactory httpFactory,
        IBackgroundJobClient backgroundJobClient,
        IApplicationDbContext context)
    {
        this._url                   = options.Value.Url ?? throw new ArgumentNullException("No options for GPT Service found!");
        this._httpClient            = httpFactory.CreateClient();
        this._backgroundJobClient   = backgroundJobClient;
        this._context               = context;
    }

    public void ProcessAnswer(QuizGPTRequestDto payload, SubmitAnswerCommand request)
    {
        _backgroundJobClient.Enqueue(() => QueueQuizAnswer(payload, request));
    }
    
    public async Task QueueQuizAnswer(QuizGPTRequestDto payload, SubmitAnswerCommand request)
    {
        QuizGPTResponseDto result = await ValidateAnswer(payload);
        await SaveAnswerToDatabase(request, result);
    }

    private async Task SaveAnswerToDatabase(SubmitAnswerCommand request, QuizGPTResponseDto result)
    {
        // write the answer to the database
        SubmittedQuizAnswer answer = new SubmittedQuizAnswer
        {
            SubmissionId        = request.SubmissionId,
            QuizQuestionId      = request.QuestionId,
            AnswerText          = request.AnswerText,
            Correct             = result.Correct,
            GPTConfidence       = result.Confidence,
            GPTExplanation      = result.Explanation
        };
        await _context.SubmittedAnswers.AddAsync(answer);
        await _context.SaveChangesAsync(CancellationToken.None);
    }


    private async Task<QuizGPTResponseDto> ValidateAnswer(QuizGPTRequestDto model)
    {
        var json        = JsonSerializer.Serialize(model);
        var data        = new StringContent(json, Encoding.UTF8, "application/json");
        var response    = await _httpClient.PostAsync(_url, data);
        
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        QuizGPTResponseDto? result = JsonSerializer.Deserialize<QuizGPTResponseDto>(responseString);

        if (result == null)
            throw new Exception("Response from GPT API was null");
        
        return result;
    }
}
