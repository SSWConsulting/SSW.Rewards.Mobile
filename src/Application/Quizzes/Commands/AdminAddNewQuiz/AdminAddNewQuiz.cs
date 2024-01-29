using Microsoft.AspNetCore.Components.Forms;
using SSW.Rewards.Shared.DTOs.Quizzes;
using SSW.Rewards.Application.Achievements.Common;

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

    public AddNewQuizCommandHandler(
        IApplicationDbContext context,
        IUserService userService,
        IQuizImageStorageProvider storage)
    {
        _context = context;
        _userService = userService;
        _storage = storage;
    }

    public async Task<int> Handle(AdminAddNewQuiz request, CancellationToken cancellationToken)
    {
        var user = _userService.GetCurrentUser();

        var dbUser = await _context.Users.FirstAsync(x => x.Id == user.Id, cancellationToken);

        var quiz = new Quiz
        {
            Title = request.NewQuiz.Title,
            Description = request.NewQuiz.Description,
            Icon = request.NewQuiz.Icon,
            IsCarousel = request.NewQuiz.IsCarousel,
            CarouselPhoto = request.NewQuiz.CarouselImage,
            ThumbnailPhoto = request.NewQuiz.ThumbnailImage,
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

        return quiz.Id;
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
