using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Users.Queries.GetUserAchievements;
using SSW.Rewards.Application.Users.Queries.GetUserRewards;
using SSW.Rewards.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Users.Queries.GetUser
{
    public class GetUserQuery : IRequest<UserViewModel>
    {
        public int Id { get; set; }

        public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;
            private readonly ILogger<GetUserQueryHandler> _logger;

            public GetUserQueryHandler(
                IMapper mapper,
                ISSWRewardsDbContext context,
                ILogger<GetUserQueryHandler> logger)
            {
                _mapper = mapper;
                _context = context;
                _logger = logger;
            }

            public async Task<UserViewModel> Handle(GetUserQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var vm = await _context.Users
                        .Where(u => u.Id == request.Id)
                        .ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (vm == null)
                    {
                        throw new NotFoundException(nameof(User), request.Id);
                    }

                    var userAchievements = await _context.UserAchievements
                                                .Include(ua => ua.Achievement)
                                                .Where(u => u.UserId == request.Id)
                                                .ProjectTo<UserAchievementViewModel>(_mapper.ConfigurationProvider)
                                                .ToListAsync();
                    var userRewards = await _context.UserRewards
                                                .Include(ur => ur.Reward)
                                                .Where(u => u.UserId == request.Id)
                                                .ProjectTo<UserRewardViewModel>(_mapper.ConfigurationProvider)
                                                .ToListAsync();

                    vm.Achievements = userAchievements;
                    vm.Rewards = userRewards;

                    var points = userAchievements.Sum(a => a.AchievementValue);
                    var spent = userRewards.Sum(r => r.RewardCost);
                    vm.Points = points;
                    vm.Balance = points - spent;

                    return vm;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                    throw e;
                }
            }
        }
    }
}