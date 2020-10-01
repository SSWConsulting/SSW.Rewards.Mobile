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

            }
        }
    }
}
