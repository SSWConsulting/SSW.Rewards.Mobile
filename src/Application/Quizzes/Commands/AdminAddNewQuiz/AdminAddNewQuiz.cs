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
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IQuizImageStorageProvider _storage;

    public AddNewQuizCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUserService userService,
        IQuizImageStorageProvider storage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userService = userService;
        _storage = storage;
    }

    public async Task<int> Handle(AdminAddNewQuiz request, CancellationToken cancellationToken)
    {
        var user = _userService.GetCurrentUser();

        var dbUser = await _context.Users.FirstAsync(x => x.Id == user.Id, cancellationToken);
        string imgUrl = string.Empty;
        
        if (request.NewQuiz.IsCarousel)
        {
            imgUrl = await UploadQuizImage(request.NewQuiz.CarouselImageFile, cancellationToken);
        }
        
        var quiz = new Quiz
        {
            Title = request.NewQuiz.Title,
            Description = request.NewQuiz.Description,
            Icon = request.NewQuiz.Icon,
            IsCarousel = request.NewQuiz.IsCarousel,
            CarouselPhoto = imgUrl,
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

    private async Task<string> UploadQuizImage(IBrowserFile file, CancellationToken cancellationToken)
    {
        var stream = file.OpenReadStream(file.Size);
        
        await using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);

        byte[] bytes = ms.ToArray();

        string filename = Guid.NewGuid().ToString();

        return await _storage.UploadCarouselImage(bytes, filename);
    }
}
