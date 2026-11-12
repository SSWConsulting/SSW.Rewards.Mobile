using Hangfire;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Application.Rewards.BackgroundJobs;

/// <summary>
/// Recurring job that resumes interrupted reward claim deliveries from the database: any record still Pending
/// whose last attempt (or creation, if never attempted) is older than <see cref="StaleAfter"/> is re-enqueued.
/// Covers a failed enqueue after the claim, a crash, or Hangfire exhausting its retries.
/// </summary>
public class RewardClaimDeliverySweepJob(
    IApplicationDbContext context,
    IBackgroundJobClient backgroundJobClient,
    IDateTime dateTime,
    ILogger<RewardClaimDeliverySweepJob> logger)
{
    public const string RecurringJobId = "reward-claim-delivery-sweep";
    public const string Schedule = "*/10 * * * *";

    // Longer than Hangfire's 5 automatic retries take, so the sweep doesn't start a second retry chain.
    public static readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(30);

    private const int BatchSize = 100;

    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    [AutomaticRetry(Attempts = 0)]
    public async Task EnqueueStaleDeliveriesAsync(CancellationToken cancellationToken)
    {
        var cutoff = dateTime.UtcNow - StaleAfter;

        var staleIds = await context.RewardClaimDeliveries
            .AsNoTracking()
            .TagWithContext("GetStalePendingDeliveries")
            .Where(d => d.Status == RewardClaimDeliveryStatus.Pending
                        && (d.LastAttemptUtc ?? d.CreatedUtc) < cutoff)
            .OrderBy(d => d.Id)
            .Select(d => d.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var id in staleIds)
        {
            RewardClaimDeliveryJob.Enqueue(backgroundJobClient, id);
        }

        if (staleIds.Count > 0)
        {
            logger.LogWarning("Re-enqueued {Count} stale reward claim deliveries: {DeliveryIds}", staleIds.Count, staleIds);
        }
    }
}
