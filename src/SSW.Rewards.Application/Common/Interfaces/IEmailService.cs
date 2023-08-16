using SSW.Rewards.Application.Common.Models;

namespace SSW.Rewards.Application.Common.Interfaces;

public interface IEmailService
{
    Task<bool> SendPhysicalRewardEmail(string to, string toName, string subject, PhysicalRewardEmail emailProps, CancellationToken cancellationToken);

    Task<bool> SendDigitalRewardEmail(string to, string toName, string subject, DigitalRewardEmail emailProps, CancellationToken cancellationToken);

    Task<bool> SendProfileDeletionRequest(string toMail, string toName, CancellationToken cancellationToken);
}
