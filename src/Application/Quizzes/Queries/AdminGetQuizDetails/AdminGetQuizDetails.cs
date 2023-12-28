using AutoMapper.QueryableExtensions;
using SSW.Rewards.Application.Quizzes.Common;

namespace SSW.Rewards.Application.Quizzes.Queries.AdminGetQuizDetails;

public class AdminGetQuizDetails : IRequest<AdminQuizDetailsDto>
{
    public int QuizId { get; set; }
    public AdminGetQuizDetails(int id)
    {
        QuizId = id;
    }
}

public sealed class Handler : IRequestHandler<AdminGetQuizDetails, AdminQuizDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    public Handler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUserService userService,
        IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<AdminQuizDetailsDto> Handle(AdminGetQuizDetails request, CancellationToken cancellationToken)
    {
        return await _context.Quizzes
                                .Include(q => q.Questions)
                                    .ThenInclude(x => x.Answers)
                                .Where(x => x.Id == request.QuizId)
                                .ProjectTo<AdminQuizDetailsDto>(_mapper.ConfigurationProvider)
                                .FirstAsync(cancellationToken);


    }
}
