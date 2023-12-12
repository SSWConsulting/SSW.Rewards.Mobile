using MediatR;
using SSW.Rewards.Application.Achievements.Common;
using SSW.Rewards.Application.Quizzes.Common;
using SSW.Rewards.Domain.Enums;

namespace SSW.Rewards.Application.Quizzes.Commands.AddNewQuiz;
public class AdminAddNewQuiz : IRequest<int>
{
    public AdminQuizDetailsDto NewQuiz { get; set; } = new();
}

public class AddNewQuizCommandHandler : IRequestHandler<AdminAddNewQuiz, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public AddNewQuizCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUserService userService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userService = userService;
    }

    public async Task<int> Handle(AdminAddNewQuiz request, CancellationToken cancellationToken)
    {
        var user = _userService.GetCurrentUser();

        var dbUser = await _context.Users.FirstAsync(x => x.Id == user.Id, cancellationToken);

        var quiz = new Quiz
        {
            Title       = request.NewQuiz.Title,
            Description = request.NewQuiz.Description,
            Icon        = request.NewQuiz.Icon,
            IsArchived  = false,
            CreatedBy   = dbUser,
            CreatedUtc  = DateTime.UtcNow
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

    private QuizQuestion CreateQuestion(AdminQuizQuestionDto dto)
    {
        var dbQuestion = new QuizQuestion
        {
            Text        = dto.Text,
            CreatedUtc  = DateTime.UtcNow,
            Answers     = dto.Answers.Select(a => new QuizAnswer
            {
                IsCorrect   = a.IsCorrect,
                Text        = a.Text
            }).ToList()
        };
        return dbQuestion;
    }
    
    private Achievement CreateQuizAchievement(AdminQuizDetailsDto dto)
    {
        return new Achievement
        {
            Icon            = dto.Icon,
            IconIsBranded   = true,
            Name            = $"Quiz: {dto.Title}",
            Code            = AchievementHelpers.GenerateCode(dto.Title, false),
            Value           = dto.Points,
            CreatedUtc      = DateTime.UtcNow
        };
    }
}
