using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizDetailsQuery
{
    public class GetQuizDetailsQuery : IRequest<QuizDetailsDto>
    {
        public int QuizId { get; set; }

        public GetQuizDetailsQuery(int id)
        {
            this.QuizId = id;
        }
        
        public sealed class Handler : IRequestHandler<GetQuizDetailsQuery, QuizDetailsDto>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;

            public Handler(
                IMapper mapper,
                ISSWRewardsDbContext context)
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
                                        //.Select(x => new QuizDetailsDto
                                        //{
                                        //    QuizId      = x.Id,
                                        //    Title       = x.Title,
                                        //    Description = x.Description,
                                        //    Questions   = x.Questions.Select(q => new QuizQuestionDto
                                        //    {
                                        //        QuestionId  = q.Id,
                                        //        Text        = q.Text,
                                        //        Answers     = q.Answers.Select(a => new QuestionAnswerDto
                                        //        {
                                        //            QuestionAnswerId = a.Id,
                                        //            Text             = a.Text
                                        //        }).ToList()
                                        //    }).ToList()
                                        //})
                                        //.AsNoTracking()
                                        .FirstOrDefaultAsync(cancellationToken);
            }
        }

    }
}
