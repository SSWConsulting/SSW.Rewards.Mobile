using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.Leaderboard.Queries.GetLeaderboardList
{
    public class GetLeaderboardListQuery : IRequest<LeaderboardListViewModel>
    {
        public sealed class Handler : IRequestHandler<GetLeaderboardListQuery, LeaderboardListViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ISSWConsultingDbContext _context;

            public Handler(
                IMapper mapper,
                ISSWConsultingDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<LeaderboardListViewModel> Handle(GetLeaderboardListQuery request, CancellationToken cancellationToken)
            {
                var users = await _context
                    .Users
                    .Include(u => u.UserAchievements)
                    .ThenInclude(ua => ua.Achievement)
                    .ProjectTo<LeaderboardUserDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                var model = new LeaderboardListViewModel
                {
                    // need to set rank outside of AutoMapper
                    Users = users
						.Where(u => !string.IsNullOrWhiteSpace(u.Name))
                        .OrderByDescending(u => u.Points)
                        .Select((u, i) =>
                        {
                            u.Rank = i + 1;
                            return u;
                        })
                };

                return model;
            }
        }
    }
}
