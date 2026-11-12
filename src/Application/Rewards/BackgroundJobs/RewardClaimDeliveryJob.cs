using Hangfire;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Common.Models;

namespace SSW.Rewards.Application.Rewards.BackgroundJobs;

/// <summary>
/// Sends the Marketing fulfilment email for a claimed reward from its <see cref="RewardClaimDelivery"/> outbox record.
/// Throws on failure so Hangfire retries; <see cref="RewardClaimDeliverySweepJob"/> re-enqueues records that are
/// still Pending after Hangfire gives up or when the enqueue after the claim never happened.
/// </summary>
public class RewardClaimDeliveryJob(
    IApplicationDbContext context,
    IRewardSender rewardSender,
    IDateTime dateTime,
    ILogger<RewardClaimDeliveryJob> logger)
{
    private const int MaxErrorLength = 1000;

    public static void Enqueue(IBackgroundJobClient backgroundJobClient, int deliveryId) =>
        backgroundJobClient.Enqueue<RewardClaimDeliveryJob>(job => job.DeliverAsync(deliveryId, CancellationToken.None));

    // Serialise deliveries so a sweep-enqueued job and a retry for the same record can't both send.
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    [AutomaticRetry(Attempts = 5)]
    public async Task DeliverAsync(int deliveryId, CancellationToken cancellationToken)
    {
        var delivery = await context.RewardClaimDeliveries
            .TagWithContext("GetDelivery")
            .FirstOrDefaultAsync(d => d.Id == deliveryId, cancellationToken);

        if (delivery == null)
        {
            logger.LogWarning("Reward claim delivery {DeliveryId} not found", deliveryId);
            return;
        }

        if (delivery.Status == RewardClaimDeliveryStatus.Delivered)
        {
            logger.LogInformation("Reward claim delivery {DeliveryId} already delivered, skipping", deliveryId);
            return;
        }

        var claim = await context.UserRewards
            .AsNoTracking()
            .TagWithContext("GetClaim")
            .Where(ur => ur.Id == delivery.UserRewardId)
            .Select(ur => new
            {
                ur.Id,
                UserName = ur.User.FullName,
                UserEmail = ur.User.Email,
                RewardName = ur.Reward.Name,
                ur.Reward.RewardType
            })
            .FirstAsync(cancellationToken);

        var notification = new RewardClaimNotification(
            claim.Id,
            claim.UserName ?? string.Empty,
            claim.UserEmail ?? string.Empty,
            claim.RewardName ?? string.Empty,
            claim.RewardType,
            FormatAddress(delivery));

        delivery.Attempts++;
        delivery.LastAttemptUtc = dateTime.UtcNow;

        bool sent;
        try
        {
            sent = await rewardSender.SendRewardAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            await RecordFailureAsync(delivery, ex.Message);
            logger.LogError(ex, "Failed to send reward claim #{ClaimId} (delivery {DeliveryId}, attempt {Attempt})", claim.Id, deliveryId, delivery.Attempts);
            throw;
        }

        if (!sent)
        {
            const string error = "Email service refused the reward notification.";
            await RecordFailureAsync(delivery, error);
            throw new InvalidOperationException($"{error} Claim #{claim.Id}, delivery {deliveryId}, attempt {delivery.Attempts}.");
        }

        delivery.Status = RewardClaimDeliveryStatus.Delivered;
        delivery.DeliveredUtc = dateTime.UtcNow;
        delivery.LastError = null;

        // The email is already accepted: don't let a host shutdown cancel recording that.
        await context.SaveChangesAsync(CancellationToken.None);
    }

    private async Task RecordFailureAsync(RewardClaimDelivery delivery, string error)
    {
        delivery.LastError = error.Length > MaxErrorLength ? error[..MaxErrorLength] : error;
        await context.SaveChangesAsync(CancellationToken.None);
    }

    private static string FormatAddress(RewardClaimDelivery delivery)
    {
        if (!string.IsNullOrWhiteSpace(delivery.FreeformAddress))
        {
            return delivery.FreeformAddress;
        }

        var street = string.Join(' ', new[] { delivery.StreetNumber, delivery.StreetName }.Where(p => !string.IsNullOrWhiteSpace(p)));

        return string.Join(", ", new[] { street, delivery.Municipality, delivery.State, delivery.PostalCode, delivery.Country }
            .Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}
