using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz;
public class SaveCompletedQuiz : IRequest<int>
{
    public int QuizId { get; set; }
}

public class SaveCompletedQuizHandler : IRequestHandler<SaveCompletedQuiz, int>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public SaveCompletedQuizHandler(
        IMapper mapper,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUserService userService)
    {
        _mapper             = mapper;
        _context            = context;
        _currentUserService = currentUserService;
        _userService        = userService;
    }

    public async Task<int> Handle(SaveCompletedQuiz request, CancellationToken cancellationToken)
    {
        var userId = await _userService.GetUserId(_currentUserService.GetUserEmail());
        // get quiz from db
        var dbQuiz = await _context.Quizzes
                                .Include(x => x.Achievement)
                                .Where(x => x.Id == request.QuizId)
                                .AsNoTracking()
                                .FirstAsync(cancellationToken);

        // add completed quiz for user
        CompletedQuiz c = new CompletedQuiz
        {
            QuizId = request.QuizId,
            UserId = userId
        };
        _context.CompletedQuizzes.Add(c);

        // add achievement for completing the quiz
        UserAchievement quizCompletedAchievement = new UserAchievement
        {
            UserId = userId,
            AwardedAt = DateTime.UtcNow,
            AchievementId = dbQuiz.AchievementId
        };
        _context.UserAchievements.Add(quizCompletedAchievement);

        await _context.SaveChangesAsync(cancellationToken);

        return c.Id;
    }
}