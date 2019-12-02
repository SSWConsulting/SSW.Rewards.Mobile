using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Common.Exceptions;
using SSW.Consulting.Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.User.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IRequest<CurrentUserViewModel>
    {
        public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ICurrentUserService _currentUserService;
            private readonly ISSWConsultingDbContext _context;

            public GetCurrentUserQueryHandler(
                IMapper mapper,
                ICurrentUserService currentUserService,
                ISSWConsultingDbContext context)
            {
                _mapper = mapper;
                _currentUserService = currentUserService;
                _context = context;
            }

            public async Task<CurrentUserViewModel> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
            {
                // need to use current user's email address to look up these details since b2c's id is not being stored
                var currentUserEmail =  _currentUserService.GetUserEmail();
                var user = await _context.Users
                    .Include(u => u.UserAchievements).ThenInclude(ua => ua.Achievement)
                    .Include(u => u.UserRewards).ThenInclude(ur => ur.Reward)
                    .Where(u => u.Email == currentUserEmail)
                    .ProjectTo<CurrentUserViewModel>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync(cancellationToken);

                if (user == null)
                {
                    throw new NotFoundException(nameof(User), currentUserEmail);
                }

                return user;
            }
        }
    }
}
