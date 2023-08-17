using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Models;
using SSW.Rewards.Application.Users.Commands.DeleteMyProfile;

namespace SSW.Rewards.Infrastructure;

public class EmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IFluentEmail fluentEmail, ILogger<EmailService> logger)
    {
        _fluentEmail = fluentEmail;
        _logger = logger;
    }

    public async Task<bool> SendDigitalRewardEmail(string to, string toName, string subject, DigitalRewardEmail emailProps, CancellationToken cancellationToken)
    {
        var template = "<p>Hi SSW Marketing</p><p>@Model.RecipientName has claimed @Model.RewardName.</p></p>Please generate a voucher code and send it to @Model.RecipientEmail.</p><p>Thanks,</p><p>SSW Rewards Notification Service</p>";

        var result = await _fluentEmail
            .To(to)
            .Subject(subject)
            .UsingTemplate(template, emailProps)
            .SendAsync(cancellationToken);

        if (result.Successful)
        {
            return true;
        }
        else
        {
            _logger.LogError("Error sending email", _fluentEmail);
            return false;
        }
    }

    public async Task<bool> SendPhysicalRewardEmail(string to, string toName, string subject, PhysicalRewardEmail emailProps, CancellationToken cancellationToken)
    {
        var template = "<p>Hi SSW Marketing</p><p>@Model.RecipientName has claimed @Model.RewardName.</p></p>Please organise to send it to @Model.RecipientAddress.</p><p>Thanks,</p><p>SSW Rewards Notification Service</p>";

        var result = await _fluentEmail
            .To(to)
            .Subject(subject)
            .UsingTemplate(template, emailProps)
            .SendAsync(cancellationToken);

        if (result.Successful)
        {
            return true;
        }
        else
        {
            _logger.LogError("Error sending email", _fluentEmail);
            return false;
        }
    }

    public async Task<bool> SendProfileDeletionRequest(DeleteProfileEmail model, CancellationToken cancellationToken)
    {
        var template = "<p>Hi SSW Rewards</p><p>{{username}} has submitted a profile deletion request.</p><p>Hi {{username}},<br><strong>Important: </strong>if you did not request this, please reply to this email letting us know ASAP, and reset your password.</p></p>1. Please delete their profile and all associated data from the following systems:<ul><li>SSW.Rewards</li><li>SSW.Identity</li></ul>.</p><p>2. Reply all 'done'.</p><p>Thanks,</p><p>SSW Rewards Notification Service</p>";

        var message = template.Replace("{{username", model.UserName);

        try
        {
            var result = await _fluentEmail
                .To(model.RewardsTeamEmail)
                .CC(model.UserEmail)
                .Subject("Profile deletion request")
                .Body(message, true)
                .SendAsync(cancellationToken);

            if (result.Successful)
            {
                return true;
            }
            else
            {
                _logger.LogError("Error sending email", _fluentEmail);
                return false;
            }
        }
        catch (Exception ex)
        {

            throw;
        }
    }
}
