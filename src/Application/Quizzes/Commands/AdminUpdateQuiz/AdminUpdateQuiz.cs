using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using MediatR;
using SSW.Rewards.Shared.DTOs.Quizzes;
using SSW.Rewards.Application.Notifications.Commands;

namespace SSW.Rewards.Application.Quizzes.Commands.AddNewQuiz;

public class AdminUpdateQuiz : IRequest<int>
{
    public QuizEditDto Quiz { get; set; } = new();
}

public class AdminUpdateQuizHandler : IRequestHandler<AdminUpdateQuiz, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly ILogger<AdminUpdateQuizHandler> _logger;

    public AdminUpdateQuizHandler(IApplicationDbContext context, ISender sender, ILogger<AdminUpdateQuizHandler> logger)
    {
        _context = context;
        _sender = sender;
        _logger = logger;
    }

    public async Task<int> Handle(AdminUpdateQuiz request, CancellationToken cancellationToken)
    {

        var dbQuiz = await _context.Quizzes
                                    .Include(q => q.Questions)
                                    .ThenInclude(r => r.Answers)
                                    .FirstAsync(x => x.Id == request.Quiz.QuizId, cancellationToken);

        dbQuiz.Title = request.Quiz.Title;
        dbQuiz.Description = request.Quiz.Description;
        dbQuiz.LastUpdatedUtc = DateTime.UtcNow;
        dbQuiz.IsArchived = request.Quiz.IsArchived;
        dbQuiz.Icon = request.Quiz.Icon;
        dbQuiz.CarouselImage = request.Quiz.CarouselImage;
        dbQuiz.ThumbnailImage = request.Quiz.ThumbnailImage;
        dbQuiz.IsCarousel = request.Quiz.IsCarousel;

        // loop through the incoming quiz's questions and add/update/delete them from the dbquiz
        foreach (var q in request.Quiz.Questions)
        {
            if (q.QuestionId == 0)
            {
                // New question. Add it.
                dbQuiz.Questions.Add(CreateQuestion(q));
                continue;
            }

            var existingQuestion = dbQuiz.Questions.First(x => x.Id == q.QuestionId);

            //Delete the question if it's marked as deleted
            //TODO: https://github.com/SSWConsulting/SSW.Rewards.Mobile/issues/773
            if (q.IsDeleted)
            {
                dbQuiz.Questions.Remove(existingQuestion);
                continue;
            }

            UpdateExistingQuestion(ref existingQuestion, q);
        }
        _context.Quizzes.Update(dbQuiz);
        await _context.SaveChangesAsync(cancellationToken);

        await NotifyUsersIfRequestedAsync(request.Quiz, dbQuiz, cancellationToken);
        return dbQuiz.Id;
    }

    private async Task NotifyUsersIfRequestedAsync(QuizEditDto quizDto, Quiz dbQuiz, CancellationToken cancellationToken)
    {
        if (!quizDto.NotifyUsers)
        {
            return;
        }

        try
        {
            string title = Truncate("Updated quiz: " + dbQuiz.Title, 100);
            string pointsText = quizDto.Points > 0 ? $" Earn {quizDto.Points} points." : string.Empty;
            string body = Truncate($"Refresh your knowledge with {dbQuiz.Title}!{pointsText}", 250);

            var command = new SendAdminNotificationCommand
            {
                Title = title,
                Body = body,
                ImageUrl = string.IsNullOrWhiteSpace(quizDto.ThumbnailImage) ? null : quizDto.ThumbnailImage
            };

            await _sender.Send(command, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send update notification for quiz {QuizId}", dbQuiz.Id);
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }

    private void UpdateExistingQuestion(ref QuizQuestion existingQuestion, QuizQuestionEditDto dto)
    {
        existingQuestion.Text = dto.Text;

        // get a list of all answerIds that exist in the dbrecord,
        // so once we finish the loop beneath, we can see what answer Ids remain
        // in the array. Any remaining Ids are answers that should be deleted.
        List<int> currentAnswerIds = existingQuestion.Answers.Select(r => r.Id).ToList();

        // loop through the answers to add/update/delete them from the existingQuestion
        foreach (var a in dto.Answers)
        {
            if (a.QuestionAnswerId > 0)
            {
                // existing answer. Update it
                currentAnswerIds.Remove(a.QuestionAnswerId);

                var existingAnswer = existingQuestion.Answers.First(x => x.Id == a.QuestionAnswerId);
                existingAnswer.Text = a.Text;
                existingAnswer.IsCorrect = a.IsCorrect;
            }
            else
            {
                // new answer. Add it.
                QuizAnswer answer = new QuizAnswer
                {
                    QuestionId = existingQuestion.Id,
                    Text = a.Text,
                    CreatedUtc = DateTime.UtcNow,
                    IsCorrect = a.IsCorrect
                };
                existingQuestion.Answers.Add(answer);
            }
        }

        foreach (int i in currentAnswerIds)
        {
            QuizAnswer answerToBeDeleted = existingQuestion.Answers.First(x => x.Id == i);
            existingQuestion.Answers.Remove(answerToBeDeleted);
        }
    }

    private QuizQuestion CreateQuestion(QuizQuestionEditDto dto)
    {
        var dbQuestion = new QuizQuestion
        {
            Text = dto.Text,
            CreatedUtc = DateTime.UtcNow,
            Answers = dto.Answers.Select(a => new QuizAnswer { IsCorrect = a.IsCorrect, Text = a.Text }).ToList()
        };
        return dbQuestion;
    }
}
