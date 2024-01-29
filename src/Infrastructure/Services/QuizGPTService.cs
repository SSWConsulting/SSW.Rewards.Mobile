using System.Text;
using System.Text.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
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

    public void ProcessAnswer(int userId, QuizGPTRequestDto payload, SubmitAnswerCommand request)
    {
        _backgroundJobClient.Enqueue(() => QueueQuizAnswer(userId, payload, request));
    }
    
    public async Task QueueQuizAnswer(int userId, QuizGPTRequestDto payload, SubmitAnswerCommand request)
    {
        QuizGPTResponseDto result = await ValidateAnswer(payload);
        await SaveAnswerToDatabase(request, result);

        // check for quiz completion and assign achievement
        await AssignAchievementIfPassed(userId, request.SubmissionId);
    }

    private async Task AssignAchievementIfPassed(int userId, int submissionId)
    {
        if (
                // order of these is important
                await UserHasCompletedQuiz(userId, submissionId) 
            &&  await AllAnswersCorrect(submissionId))
        {
            await PassQuiz(submissionId);
            await AssignAchievement(userId, submissionId);
        }
    }

    private async Task<bool> UserHasAchievement(int userId, int achievementId)
    {
        return await _context.UserAchievements
            .Where(ua => ua.UserId == userId && ua.AchievementId == achievementId)
            .AnyAsync();
    }
    private async Task PassQuiz(int submissionId)
    {
        CompletedQuiz dbCompletedQuiz = await _context.CompletedQuizzes
            .FirstAsync(cq => cq.Id == submissionId);

        dbCompletedQuiz.Passed = true;

        _context.CompletedQuizzes.Update(dbCompletedQuiz);
        await _context.SaveChangesAsync(CancellationToken.None);
    }
    private async Task AssignAchievement(int userId, int submissionId)
    {
        int achievementId = await _context.CompletedQuizzes
            .Where(cq => cq.Id == submissionId && cq.UserId == userId)
            .Select(cq => cq.Quiz.AchievementId)
            .FirstAsync(CancellationToken.None);

        if (await UserHasAchievement(userId, achievementId))
            return;
        
        UserAchievement userAchievement = new UserAchievement
        {
            UserId          = userId,
            AchievementId   = achievementId,
            AwardedAt       = DateTime.Now,
        };
        await _context.UserAchievements.AddAsync(userAchievement);
        await _context.SaveChangesAsync(CancellationToken.None);
    }
    private async Task<bool> AllAnswersCorrect(int submissionId)
    {
        CompletedQuiz dbUserQuiz = await _context.CompletedQuizzes
            .Where(cq => cq.Id == submissionId)
            .Include(cq => cq.Answers)
            .FirstAsync(CancellationToken.None);

        return dbUserQuiz.Answers.All(a => a.Correct);
    }
    private async Task<bool> UserHasCompletedQuiz(int userId, int submissionId)
    {
        var quizSubmission = await _context.CompletedQuizzes
                .Where(cq => 
                        cq.UserId == userId 
                    &&  cq.Id == submissionId)
                .Include(cq => cq.Quiz)
                    .ThenInclude(q => q.Questions)
                .Include(cq => cq.Answers)
            .FirstAsync(CancellationToken.None);

        return quizSubmission.Answers.Count == quizSubmission.Quiz.Questions.Count;
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
