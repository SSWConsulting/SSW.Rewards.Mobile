using AutoMapper;
using MediatR;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizListForUserQuery
{
    public class GetQuizListForUserQuery : IRequest<IEnumerable<QuizDto>>
    {

        public sealed class Handler : IRequestHandler<GetQuizListForUserQuery, IEnumerable<QuizDto>>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;
            private readonly ICurrentUserService _userService;

            public Handler(
                IMapper mapper,
                ISSWRewardsDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<IEnumerable<QuizDto>> Handle(GetQuizListForUserQuery request, CancellationToken cancellationToken)
            {
                return new List<QuizDto>
                {
                    new QuizDto
                    {
                        Id = 1,
                        Title = "Angular",
                        Description = "Sick Angular quiz!",
                        Icon = Domain.Entities.Icons.Angular,
                        Passed = false
                    }
                };
            }
        }

    }
}
