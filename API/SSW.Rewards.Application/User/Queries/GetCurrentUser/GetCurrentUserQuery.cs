using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
                // need to use current user's email address to look up these details since b2c's id is not being stored
                string currentUserEmail =  _currentUserService.GetUserEmail();

                try
                {
	                CurrentUserViewModel user = await _context.Users
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
                catch (NotFoundException nfex)
                {
					// nothing to do here - just rethrow the exception from above
					_logger.LogError(nfex, "Unable to find current user with email {currentUserEmail}", currentUserEmail);
	                throw;
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while trying to find current user with email {currentUserEmail}", currentUserEmail);
                    throw new NotFoundException(nameof(User), currentUserEmail);
                }
            }
        }
    }
}
