using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Models;
using SSW.Rewards.Application.Users.Commands.Common;

namespace SSW.Rewards.Infrastructure;

public class EmailService(IFluentEmail fluentEmail, ILogger<EmailService> logger) : IEmailService
{
    private const string TemplatePath = "SSW.Rewards.Infrastructure.Services.EmailTemplates.{0}.cshtml";
    
    public async Task<bool> SendDigitalRewardEmail(string to, string toName, string subject,
        DigitalRewardEmail emailProps, CancellationToken cancellationToken)
    {
        var result = await fluentEmail
            .To(to)
            .Subject(subject)
            .UsingTemplateFromEmbedded(string.Format(TemplatePath, "DigitalReward"), emailProps, GetType().Assembly)
            .SendAsync(cancellationToken);

        if (!result.Successful)
        {
            logger.LogError("Error sending email", fluentEmail);
        }

        return result.Successful;
    }

    public async Task<bool> SendPhysicalRewardEmail(string to, string toName, string subject,
        PhysicalRewardEmail emailProps, CancellationToken cancellationToken)
    {
        var result = await fluentEmail
            .To(to)
            .Subject(subject)
            .UsingTemplateFromEmbedded(string.Format(TemplatePath, "PhysicalReward"), emailProps, GetType().Assembly)
            .SendAsync(cancellationToken);

        if (!result.Successful)
        {
            logger.LogError("Error sending email", fluentEmail);
        }

        return result.Successful;
    }


    public async Task<bool> SendProfileDeletionRequest(DeleteProfileEmail model, CancellationToken cancellationToken)
    {
        var result = await fluentEmail
            .To(model.RewardsTeamEmail)
            .CC(model.UserEmail)
            .Subject("Profile deletion request")
            .UsingTemplateFromEmbedded(string.Format(TemplatePath, "ProfileDeletionRequest"), model, GetType().Assembly)
            .SendAsync(cancellationToken);
        
        if (!result.Successful)
        {
            logger.LogError("Error sending email", fluentEmail);
        }

        return result.Successful;
    }

    public async Task<bool> SendProfileDeletionConfirmation(DeleteProfileEmail model, CancellationToken cancellationToken)
    {
        var result = await fluentEmail
            .To(model.RewardsTeamEmail)
            .CC(model.UserEmail)
            .Subject("Re: Profile deletion request")
            .UsingTemplateFromEmbedded(string.Format(TemplatePath, "ProfileDeletionConfirmation"), model, GetType().Assembly)
            .SendAsync(cancellationToken);

        if (!result.Successful)
        {
            logger.LogError("Error sending email", fluentEmail);
        }

        return result.Successful;
    }

    public async Task<bool> SendFormCompletionPointsReceivedEmail(string to, FormCompletionPointsReceivedEmail model, CancellationToken cancellationToken)
    {
        var result = await fluentEmail
            .To(to)
            .Subject("SSW.Rewards - Points received for form completion!")
            .UsingTemplateFromEmbedded(string.Format(TemplatePath, "FormCompletionPointsReceived"), model, GetType().Assembly)
            .SendAsync();

        if (!result.Successful)
        {
            logger.LogError("Error sending email", fluentEmail);
        }

        return result.Successful;
    }
    
    public async Task<bool> SendFormCompletionCreateAccountEmail(string to, FormCompletionCreateAccountEmail model, CancellationToken cancellationToken)
    {
        var result = await fluentEmail
            .To(to)
            .Subject("SSW.Rewards - Account created for form completion!")
            .UsingTemplateFromEmbedded(string.Format(TemplatePath, "FormCompletionCreateAccount"), model, GetType().Assembly)
            .SendAsync();

        if (!result.Successful)
        {
            logger.LogError("Error sending email", fluentEmail);
        }

        return result.Successful;
    }
}
