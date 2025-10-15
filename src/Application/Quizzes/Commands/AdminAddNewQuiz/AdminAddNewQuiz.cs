using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using MediatR;
using SSW.Rewards.Shared.DTOs.Quizzes;
using SSW.Rewards.Application.Achievements.Common;
using SSW.Rewards.Application.Notifications.Commands;

namespace SSW.Rewards.Application.Quizzes.Commands.AddNewQuiz;

public class AdminAddNewQuiz : IRequest<int>
{
    public QuizEditDto NewQuiz { get; set; } = new();
}

public class AddNewQuizCommandHandler : IRequestHandler<AdminAddNewQuiz, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserService _userService;
    private readonly IQuizImageStorageProvider _storage;
    private readonly ISender _sender;
    private readonly ILogger<AddNewQuizCommandHandler> _logger;

    public AddNewQuizCommandHandler(
        IApplicationDbContext context,
        IUserService userService,
        IQuizImageStorageProvider storage,
        ISender sender,
        ILogger<AddNewQuizCommandHandler> logger)
    {
        _context = context;
        _userService = userService;
        _storage = storage;
        _sender = sender;
        _logger = logger;
    }

    public async Task<int> Handle(AdminAddNewQuiz request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetCurrentUser(cancellationToken);

        var dbUser = await _context.Users.FirstAsync(x => x.Id == user.Id, cancellationToken);

        var quiz = new Quiz
        {
            Title = request.NewQuiz.Title,
            Description = request.NewQuiz.Description,
            Icon = request.NewQuiz.Icon,
            IsCarousel = request.NewQuiz.IsCarousel,
            CarouselImage = request.NewQuiz.CarouselImage,
            ThumbnailImage = request.NewQuiz.ThumbnailImage,
            IsArchived = false,
            CreatedBy = dbUser,
            CreatedUtc = DateTime.UtcNow
        };

        foreach (var question in request.NewQuiz.Questions)
        {
            quiz.Questions.Add(CreateQuestion(question));
        }

        quiz.Achievement = CreateQuizAchievement(request.NewQuiz);

        _context.Quizzes.Add(quiz);

        await _context.SaveChangesAsync(cancellationToken);

        await NotifyUsersOfNewQuizAsync(request.NewQuiz, quiz, cancellationToken);

        return quiz.Id;
    }

    private async Task NotifyUsersOfNewQuizAsync(QuizEditDto quizDto, Quiz quiz, CancellationToken cancellationToken)
    {
        if (!quizDto.NotifyUsers)
        {
            return;
        }

        try
        {
            string title = Truncate("New quiz: " + quiz.Title, 100);
            string pointsText = quizDto.Points > 0 ? $" Earn {quizDto.Points} points." : string.Empty;
            string body = Truncate($"Take the {quiz.Title} quiz today!{pointsText}", 250);

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
            _logger.LogError(ex, "Failed to send notification for new quiz {QuizId}", quiz.Id);
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

    private QuizQuestion CreateQuestion(QuizQuestionEditDto dto)
    {
        var dbQuestion = new QuizQuestion
        {
            Text = dto.Text,
            CreatedUtc = DateTime.UtcNow,
            Answers = dto.Answers.Select(a => new QuizAnswer
            {
                IsCorrect = a.IsCorrect,
                Text = a.Text
            }).ToList()
        };
        return dbQuestion;
    }

    private Achievement CreateQuizAchievement(QuizEditDto dto)
    {
        return new Achievement
        {
            Icon = dto.Icon,
            IconIsBranded = true,
            Name = $"Quiz: {dto.Title}",
            Code = AchievementHelpers.GenerateCode(dto.Title, false),
            Value = dto.Points,
            CreatedUtc = DateTime.UtcNow
        };
    }
}
