using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Application.Quizzes.Queries.GetAdminQuizList;

public class GetAdminQuizListQuery : IRequest<IEnumerable<QuizDetailsDto>> { }

public class GetAdminQuizListQueryHandler : IRequestHandler<GetAdminQuizListQuery, IEnumerable<QuizDetailsDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetAdminQuizListQueryHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<IEnumerable<QuizDetailsDto>> Handle(GetAdminQuizListQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Quizzes
            .ProjectTo<QuizDetailsDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
