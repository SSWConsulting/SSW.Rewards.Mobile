using AutoMapper.QueryableExtensions;
using SSW.Rewards.Application.Quizzes.Common;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizDetails;
public class GetQuizDetails : IRequest<QuizDetailsDto>
{
    public int QuizId { get; set; }

    public GetQuizDetails(int id)
    {
        QuizId = id;
    }
}

public class GetQuizDetailsHandler : IRequestHandler<GetQuizDetails, QuizDetailsDto>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public GetQuizDetailsHandler(
        IMapper mapper,
        IApplicationDbContext context)
    {
        _mapper     = mapper;
        _context    = context;
    }

    public async Task<QuizDetailsDto> Handle(GetQuizDetails request, CancellationToken cancellationToken)
    {
        return await _context.Quizzes
                                .Include(x => x.Questions)
                                    .ThenInclude(x => x.Answers)
                                .Where(x => x.Id == request.QuizId)
                                .ProjectTo<QuizDetailsDto>(_mapper.ConfigurationProvider)
                                .FirstAsync(cancellationToken);
    }
}