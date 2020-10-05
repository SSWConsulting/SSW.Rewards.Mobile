using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Infrastructure
{
    public class RewardSender : IRewardSender
    {
        private readonly ISSWRewardsDbContext _context;

        public RewardSender(ISSWRewardsDbContext context)
        {
            _context = context;
        }

        public Task SendReward(User user, Reward reward)
        {
            return SendRewardAsync(user, reward, CancellationToken.None);
        }

        public async Task SendRewardAsync(User user, Reward reward, CancellationToken cancellationToken)
        {
            if (reward.RewardType == RewardType.Physical)
            {

            }
            else
            {
                // TODO: Replace this with logic that:
                // 1. Determines if it is a free ticket
                // 2. If yes, generate with the appropriate API (currently eventbrite) and send to user
                // 3. If no, send notification to Marketing for appropriate action
            }
        }
    }
}
