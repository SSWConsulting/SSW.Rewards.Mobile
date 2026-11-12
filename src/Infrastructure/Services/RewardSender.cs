using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Models;
using SSW.Rewards.Enums;

namespace SSW.Rewards.Infrastructure;

public class RewardSender : IRewardSender
{
    // TODO: These values are hard coded here but will be replaced once rewards are sent automatically.
    private const string RecipientEmail = "SSWMarketing@ssw.com.au";
    private const string RecipientName = "SSW Marketing";

    private readonly IEmailService _emailService;
    private readonly ILogger<RewardSender> _logger;

    public RewardSender(IEmailService emailService, ILogger<RewardSender> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> SendRewardAsync(RewardClaimNotification claim, CancellationToken cancellationToken)
    {
        // The claim number makes a repeated email recognisable as the same claim.
        string emailSubject = $"SSW Rewards - Reward Notification - Claim #{claim.ClaimId}";

        bool rewardSentSuccessfully;

        if (claim.RewardType == RewardType.Physical)
        {
            var emailProps = new PhysicalRewardEmail
            {
                ClaimId = claim.ClaimId,
                RecipientAddress = claim.Address,
                RecipientEmail = claim.UserEmail,
                RecipientName = claim.UserName,
                RewardName = claim.RewardName
            };

            rewardSentSuccessfully = await _emailService.SendPhysicalRewardEmail(RecipientEmail, RecipientName, emailSubject, emailProps, cancellationToken);
        }
        else
        {
            // TODO: Replace this with logic that:
            // 1. Determines if it is a free ticket
            // 2. If yes, generate with the appropriate API (currently eventbrite) and send to user
            // 3. If no, send notification to Marketing for appropriate action
            var emailProps = new DigitalRewardEmail
            {
                ClaimId = claim.ClaimId,
                RecipientName = claim.UserName,
                RecipientEmail = claim.UserEmail,
                RewardName = claim.RewardName,
                VoucherCode = "Please generate for user"
            };

            rewardSentSuccessfully = await _emailService.SendDigitalRewardEmail(RecipientEmail, RecipientName, emailSubject, emailProps, cancellationToken);
        }

        if (!rewardSentSuccessfully)
        {
            _logger.LogError("Email service refused the reward notification for claim #{ClaimId}", claim.ClaimId);
        }

        return rewardSentSuccessfully;
    }
}
