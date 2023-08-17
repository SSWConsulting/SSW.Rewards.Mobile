using SSW.Rewards.Application.Common.Models;
using SSW.Rewards.Application.Users.Commands.DeleteMyProfile;

namespace SSW.Rewards.Application.Common.Interfaces;

public interface IEmailService
{
    Task<bool> SendPhysicalRewardEmail(string to, string toName, string subject, PhysicalRewardEmail emailProps, CancellationToken cancellationToken);

    Task<bool> SendDigitalRewardEmail(string to, string toName, string subject, DigitalRewardEmail emailProps, CancellationToken cancellationToken);

    Task<bool> SendProfileDeletionRequest(DeleteProfileEmail model, CancellationToken cancellationToken);
}
