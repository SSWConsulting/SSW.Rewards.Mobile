using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Models;
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
        private readonly IEmailService _emailService;
        private readonly ILogger<RewardSender> _logger;

        public RewardSender(ISSWRewardsDbContext context, IEmailService emailService, ILogger<RewardSender> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public Task SendReward(User user, Reward reward)
        {
            return SendRewardAsync(user, reward, CancellationToken.None);
        }

        public async Task SendRewardAsync(User user, Reward reward, CancellationToken cancellationToken)
        {
            var entity = new UserReward
            {
                AwardedAt = DateTime.Now,
                Reward = reward,
                User = user
            };

            _context.UserRewards.Add(entity);

            string emailSubject = "SSW Rewards - Reward Notification";

            bool rewardSentSuccessully;

            if (reward.RewardType == RewardType.Physical)
            {
                PhysicalRewardEmail emailProps = new PhysicalRewardEmail
                {
                    RecipientAddress = user.Address.ToString(),
                    RecipientName = user.FullName,
                    RewardName = reward.Name
                };

                rewardSentSuccessully = await _emailService.SendPhysicalRewardEmail(user.Email, user.FullName, emailSubject, emailProps, cancellationToken);
            }
            else
            {
                // TODO: Replace this with logic that:
                // 1. Determines if it is a free ticket
                // 2. If yes, generate with the appropriate API (currently eventbrite) and send to user
                // 3. If no, send notification to Marketing for appropriate action

                string recipient = "marketing@ssw.com.au";
                string voucherCode = "Please generate for user";

                //end TODO

                DigitalRewardEmail emailProps = new DigitalRewardEmail
                {
                    RecipientName = user.FullName,
                    RewardName = reward.Name,
                    VoucherCode = voucherCode
                };

                rewardSentSuccessully = await _emailService.SendDigitalRewardEmail(recipient, user.FullName, emailSubject, emailProps, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            if(!rewardSentSuccessully)
            {
                _logger.LogError("Error sending email reward notification.", reward, user);
            }
        }
    }
}
