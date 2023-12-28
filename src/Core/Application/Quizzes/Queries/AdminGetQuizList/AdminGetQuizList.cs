namespace SSW.Rewards.Application.Quizzes.Queries.AdminGetQuizList;

public class AdminGetQuizList : IRequest<IEnumerable<AdminQuizDto>>
{

}

public sealed class Handler : IRequestHandler<AdminGetQuizList, IEnumerable<AdminQuizDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    public Handler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUserService userService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userService = userService;
    }

    public async Task<IEnumerable<AdminQuizDto>> Handle(AdminGetQuizList request, CancellationToken cancellationToken)
    {
        return await _context.Quizzes
                                .Include(q => q.Questions)
                                    .ThenInclude(x => x.Answers)
                                .Where(x => !x.IsArchived)
                                .OrderBy(q => q.Title)
                                .AsNoTracking()
                                .Select(quiz => new AdminQuizDto
                                {
                                    Id = quiz.Id,
                                    Title = quiz.Title,
                                    Description = quiz.Description,
                                    DateCreated = quiz.CreatedUtc,
                                    Points = quiz.Achievement.Value
                                })
                                .ToListAsync(cancellationToken);


    }
}
