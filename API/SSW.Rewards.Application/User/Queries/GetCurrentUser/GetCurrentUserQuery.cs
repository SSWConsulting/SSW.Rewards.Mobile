using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Common.Extensions;
using SSW.Rewards.Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.User.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IRequest<CurrentUserViewModel>
    {
        public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserViewModel>
        {
            private readonly ILogger<GetCurrentUserQueryHandler> _logger;
            private readonly IMapper _mapper;
            private readonly ICurrentUserService _currentUserService;
            private readonly ISSWRewardsDbContext _context;

            public GetCurrentUserQueryHandler(
                ILogger<GetCurrentUserQueryHandler> logger,
                IMapper mapper,
                ICurrentUserService currentUserService,
                ISSWRewardsDbContext context)
            {
                _logger = logger;
                _mapper = mapper;
                _currentUserService = currentUserService;
                _context = context;
            }

            public async Task<CurrentUserViewModel> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
            {
                CurrentUserViewModel user = await GetCurrentUserVm(cancellationToken);

                if (user == null)
                {
                    var newUser = new Domain.Entities.User();
                    await _context.Users.AddAsync(newUser, cancellationToken);
                    _mapper.Map(_currentUserService, newUser);
                    await _context.SaveChangesAsync(cancellationToken);

                    user = await GetCurrentUserVm(cancellationToken);
                }

                if (user.IsStaff())
                {
                    var achievement = await _context.StaffMembers
                        .Include(s => s.StaffAchievement)
                        .Where(s => s.Email == user.Email)
                        .Select(s => s.StaffAchievement)
                        .AsNoTracking()
                        .SingleOrDefaultAsync(cancellationToken);

                    if (achievement?.Code != null)
                    {
                        user.QRCode = achievement.Code;
                    }
                }

                return user;
            }

            private async Task<CurrentUserViewModel> GetCurrentUserVm(CancellationToken cancellationToken)
            {
                // need to use current user's email address to look up these details since b2c's id is not being stored
                string currentUserEmail = _currentUserService.GetUserEmail();

                return await _context.Users
                        .Include(u => u.UserAchievements).ThenInclude(ua => ua.Achievement)
                        .Include(u => u.UserRewards).ThenInclude(ur => ur.Reward)
                        .Where(u => u.Email == currentUserEmail)
                        .ProjectTo<CurrentUserViewModel>(_mapper.ConfigurationProvider)
                        .SingleOrDefaultAsync(cancellationToken);
            }
        }
    }
}
