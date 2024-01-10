using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Application.Quizzes.Queries.GetAdminQuizDetails;

public class GetAdminQuizDetailsQuery : IRequest<QuizEditDto>
{
    public int QuizId { get; set; }

    public GetAdminQuizDetailsQuery(int id)
    {
        QuizId = id;
    }
}

public sealed class Handler : IRequestHandler<GetAdminQuizDetailsQuery, QuizEditDto>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public Handler(
        IMapper mapper,
        IApplicationDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<QuizEditDto> Handle(GetAdminQuizDetailsQuery request, CancellationToken cancellationToken)
    {
#pragma warning disable CS8603 // Possible null reference return. - Protected by validator
        return await _context.Quizzes
            .Include(x => x.Questions)
                .ThenInclude(x => x.Answers)
            .Where(x => x.Id == request.QuizId)
            .ProjectTo<QuizEditDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
#pragma warning restore CS8603 // Possible null reference return.
    }
}
