using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Models;
using SSW.Rewards.Application.Users.Commands.Common;

namespace SSW.Rewards.Infrastructure;

public class EmailService(IFluentEmail fluentEmail, ILogger<EmailService> logger) : IEmailService
{
    public async Task<bool> SendDigitalRewardEmail(string to, string toName, string subject,
        DigitalRewardEmail emailProps, CancellationToken cancellationToken)
    {
        var template =
            "<p>Hi SSW Marketing</p><p>@Model.RecipientName has claimed @Model.RewardName.</p></p>Please generate a voucher code and send it to @Model.RecipientEmail.</p><p>Thanks,</p><p>SSW Rewards Notification Service</p>";

        var result = await fluentEmail
            .To(to)
            .Subject(subject)
            .UsingTemplate(template, emailProps)
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
        const string template = "<p>Hi SSW Marketing</p><p>@Model.RecipientName has claimed @Model.RewardName.</p></p><ol><li>Please organise to send it to @Model.RecipientAddress</li><li>Please send an email with tracking info to @Model.RecipientEmail</li></ol></p><p>Thanks,</p><p>SSW Rewards Notification Service</p>";

        var result = await fluentEmail
            .To(to)
            .Subject(subject)
            .UsingTemplate(template, emailProps)
            .SendAsync(cancellationToken);

        if (!result.Successful)
        {
            logger.LogError("Error sending email", fluentEmail);
        }

        return result.Successful;
    }


    public async Task<bool> SendProfileDeletionRequest(DeleteProfileEmail model, CancellationToken cancellationToken)
    {
        var template =
            "<p><span  style=\"color: CC4141; font-weight: bold;\">Hi {{username}},</span><br><strong>Important: </strong>if you did not request this, please reply to this email letting us know ASAP, and reset your password.</p><p style=\"color: CC4141; font-weight: bold;\">Hi SSW Rewards</p><p>{{username}} has submitted a profile deletion request.</p><p>1. Please delete their profile and all associated data from the following systems:<ul><li>SSW.Rewards</li><li>SSW.Identity</li></ul></p><p>2. Reply all 'done'.</p><p>Thanks,</p><p>SSW Rewards Notification Service</p>";

        var message = template.Replace("{{username}}", model.UserName);

        var result = await fluentEmail
            .To(model.RewardsTeamEmail)
            .CC(model.UserEmail)
            .Subject("Profile deletion request")
            .Body(message, true)
            .SendAsync(cancellationToken);
        
        if (!result.Successful)
        {
            logger.LogError("Error sending email", fluentEmail);
        }

        return result.Successful;
    }

    public async Task<bool> SendProfileDeletionConfirmation(DeleteProfileEmail model, string deletionRequestDate,
        CancellationToken cancellationToken)
    {
        var template =
            "<p style=\"color: CC4141; font-weight: bold;\">Hi {{username}}</p><p><blockquote>&gt; 1. Please delete their profile and all associated data from the following systems:<ul><li>SSW.Rewards</li><li>SSW.Identity</li></ul></blockquote>✅ Done - profile has been deleted</p><p><strong>Important: </strong>You can no longer accumulate points and are ineligible to claim rewards or win prizes.</p><p>You can reply to this email if you have any concerns, and feel free to sign up again any time!</p><p>Thanks,</p><p>SSW Rewards Notification Service</p><hr/><strong>From:</strong> Verify [SSW] &lt;verify@ssw.com.au&gt;<br><strong>Date:</strong> {{originalDate}}<br><strong>To: </strong> SSW Rewards &lt;SSWRewards@ssw.com.au&gt;<br><strong>Cc: </strong> {{username}} &lt;{{email}}&gt;<br><strong>Subject:</strong> Profile deletion request<br><br><p><span  style=\"color: CC4141; font-weight: bold;\">Hi {{username}},</span><br><strong>Important: </strong>if you did not request this, please reply to this email letting us know ASAP, and reset your password.</p><p style=\"color: CC4141; font-weight: bold;\">Hi SSW Rewards</p><p>{{username}} has submitted a profile deletion request.</p><p>1. Please delete their profile and all associated data from the following systems:<ul><li>SSW.Rewards</li><li>SSW.Identity</li></ul></p><p>2. Reply all 'done'.</p><p>Thanks,</p><p>SSW Rewards Notification Service</p>";

        var message = template.Replace("{{username}}", model.UserName);

        message = message.Replace("{{email}}", model.UserEmail);

        message = message.Replace("{{originalDate}}", deletionRequestDate);


        var result = await fluentEmail
            .To(model.RewardsTeamEmail)
            .CC(model.UserEmail)
            .Subject("Re: Profile deletion request")
            .Body(message, true)
            .SendAsync(cancellationToken);

        if (!result.Successful)
        {
            logger.LogError("Error sending email", fluentEmail);
        }

        return result.Successful;
    }

    public async Task<bool> SendFormCompletionPointsReceivedEmail(string to, FormCompletionPointsReceivedEmail model, CancellationToken cancellationToken)
    {
        const string template = """
                                <!DOCTYPE html>

                                <html>
                                <head>
                                    <title></title>
                                </head>
                                <body>
                                    <div>
                                        Hi @Model.UserName,
                                        <br />
                                        Thanks for completing the form! 
                                        <br />
                                        You have received @Model.Points points.
                                    </div>
                                </body>
                                </html>
                                """;
        
        var result = await fluentEmail
            .To(to)
            .Subject("SSW.Rewards - Points received for form completion!")
            .UsingTemplate(template, model)
            .SendAsync();

        if (!result.Successful)
        {
            logger.LogError("Error sending email", fluentEmail);
        }

        return result.Successful;
    }
    
    public async Task<bool> SendFormCompletionCreateAccountEmail(string to, FormCompletionCreateAccountEmail model, CancellationToken cancellationToken)
    {
        const string template = """
                       <!DOCTYPE html>
                       
                       <html>
                       <head>
                           <title></title>
                       </head>
                       <body>
                           <div>
                                Hi @Model.Name!
                                <br />
                                Thanks for completing the form!
                                <br />
                                <a href="https://onelink.to/4feu6v">Click here</a> to download SSW.Rewards and claim your achievement! 
                           </div>
                       </body>
                       </html>
                   """;


        var result = await fluentEmail
            .To(to)
            .Subject("SSW.Rewards - Account created for form completion!")
            .UsingTemplate(template, model)
            .SendAsync();

        if (!result.Successful)
        {
            logger.LogError("Error sending email", fluentEmail);
        }

        return result.Successful;
    }
}
