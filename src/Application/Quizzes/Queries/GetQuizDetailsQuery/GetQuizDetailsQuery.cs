using AutoMapper.QueryableExtensions;
using Shared.DTOs.Quizzes;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizDetailsQuery;

public class GetQuizDetailsQuery : IRequest<QuizDetailsDto>
{
    public int QuizId { get; set; }

    public GetQuizDetailsQuery(int id)
    {
        QuizId = id;
    }
    
    public sealed class Handler : IRequestHandler<GetQuizDetailsQuery, QuizDetailsDto>
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

        public async Task<QuizDetailsDto> Handle(GetQuizDetailsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Quizzes
                                    .Include(x => x.Questions)
                                        .ThenInclude(x => x.Answers)
                                    .Where(x => x.Id == request.QuizId)
                                    .ProjectTo<QuizDetailsDto>(_mapper.ConfigurationProvider)
                                    .FirstOrDefaultAsync(cancellationToken);
        }
    }

}
