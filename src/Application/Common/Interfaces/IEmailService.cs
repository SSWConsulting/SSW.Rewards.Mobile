using SSW.Rewards.Application.Common.Models;
using SSW.Rewards.Application.Users.Commands.Common;

namespace SSW.Rewards.Application.Common.Interfaces;

public interface IEmailService
{
    Task<bool> SendPhysicalRewardEmail(string to, string toName, string subject, PhysicalRewardEmail emailProps,
        CancellationToken cancellationToken);

    Task<bool> SendDigitalRewardEmail(string to, string toName, string subject, DigitalRewardEmail emailProps,
        CancellationToken cancellationToken);

    Task<bool> SendProfileDeletionRequest(DeleteProfileEmail model, CancellationToken cancellationToken);

    Task<bool> SendProfileDeletionConfirmation(DeleteProfileEmail model, CancellationToken cancellationToken);

    Task<bool> SendFormCompletionPointsReceivedEmail(string to, FormCompletionPointsReceivedEmail model, CancellationToken cancellationToken);

    Task<bool> SendFormCompletionCreateAccountEmail(string to, FormCompletionCreateAccountEmail model, CancellationToken cancellationToken);
}
