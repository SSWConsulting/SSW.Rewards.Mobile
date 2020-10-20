using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.User.Queries.GetUser
{
    public class GetUserQuery : IRequest<UserViewModel>
    {
        public int Id { get; set; }

        public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;

            public GetUserQueryHandler(
                IMapper mapper,
                ISSWRewardsDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<UserViewModel> Handle(GetUserQuery request, CancellationToken cancellationToken)
            {
                var user = await _context.Users
                    .Include(u => u.UserAchievements).ThenInclude(ua => ua.Achievement)
                    .Include(u => u.UserRewards).ThenInclude(ur => ur.Reward)
                    .Where(u => u.Id == request.Id)
                    .ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync(cancellationToken);

                if (user == null)
                {
                    throw new NotFoundException(nameof(User), request.Id);
                }

                return user;
            }
        }
    }
}
