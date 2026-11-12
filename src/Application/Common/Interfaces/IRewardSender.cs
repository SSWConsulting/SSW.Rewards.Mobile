using SSW.Rewards.Application.Common.Models;

namespace SSW.Rewards.Application.Common.Interfaces;

public interface IRewardSender
{
    /// <summary>
    /// Sends the fulfilment request for a claimed reward to SSW Marketing.
    /// </summary>
    /// <returns><c>true</c> when the email was accepted; <c>false</c> when it was refused.</returns>
    Task<bool> SendRewardAsync(RewardClaimNotification claim, CancellationToken cancellationToken);
}
