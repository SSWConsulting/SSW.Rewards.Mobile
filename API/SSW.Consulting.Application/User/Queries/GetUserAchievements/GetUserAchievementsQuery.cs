using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.User.Queries.GetUserAchievements
{
    public class GetUserAchievementsQuery : IRequest<UserAchievementsViewModel>
    {
        public int UserId { get; set; }

        public class GetUserAchievementsQueryHandler : IRequestHandler<GetUserAchievementsQuery, UserAchievementsViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ISSWConsultingDbContext _context;

            public GetUserAchievementsQueryHandler(
                IMapper mapper,
                ISSWConsultingDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<UserAchievementsViewModel> Handle(GetUserAchievementsQuery request, CancellationToken cancellationToken)
            {
                var userAchievements = await _context.Achievements
                    .Include(a => a.UserAchievements)
                    .Select(a => new JoinedUserAchievement
                    {
                        Achievement = a,
                        UserAchievement = a.UserAchievements.FirstOrDefault(ua => ua.UserId == request.UserId)
                    })
                    .ProjectTo<UserAchievementViewModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return new UserAchievementsViewModel
                {
                    UserId = request.UserId,
                    Points = userAchievements
                        .Where(ua => ua.Complete)
                        .Sum(ua => ua.AchievementValue),
                    UserAchievements = userAchievements
                };
            }
        }
    }
}
