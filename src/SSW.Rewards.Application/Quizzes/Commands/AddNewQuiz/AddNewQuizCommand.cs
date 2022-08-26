using SSW.Rewards.Application.Achievements.Common;
using SSW.Rewards.Application.Quizzes.Common;
using SSW.Rewards.Domain.Enums;

namespace SSW.Rewards.Application.Quizzes.Commands.AddNewQuiz;
public class AddNewQuizCommand : IRequest<int>
{
    public List<QuizQuestionDto> Questions { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public int Points { get; set; }

    public Icons Icon { get; set; }
}

public class AddNewQuizCommandHandler : IRequestHandler<AddNewQuizCommand, int>
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

    public async Task<int> Handle(AddNewQuizCommand request, CancellationToken cancellationToken)
    {
        var user = _userService.GetCurrentUser();

        var dbUser = await _context.Users.FindAsync(user.Id, cancellationToken);

        var quiz = new Quiz
        {
            Title       = request.Title,
            Description = request.Description,
            Icon        = request.Icon,
            IsArchived  = false,
            CreatedBy   = dbUser
        };

        foreach (var question in request.Questions)
        {
            var dbQuestion = new QuizQuestion
            {
                Text = question.Text
            };

            foreach (var answer in question.Answers)
            {
                dbQuestion.Answers.Add(new QuizAnswer
                {
                    IsCorrect = answer.IsCorrect,
                    Text = answer.Text
                });
            }

            quiz.Questions.Add(dbQuestion);
        }

        quiz.Achievement = new Achievement
        {
            Icon            = request.Icon,
            IconIsBranded   = true,
            Name            = $"Quiz: {request.Title}",
            Code            = AchievementHelpers.GenerateCode(request.Title, false),
            Value           = request.Points
        };

        _context.Quizzes.Add(quiz);

        await _context.SaveChangesAsync(cancellationToken);

        return quiz.Id;
    }
}
