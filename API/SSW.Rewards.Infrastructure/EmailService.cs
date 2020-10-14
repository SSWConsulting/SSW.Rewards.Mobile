using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Infrastructure
{
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
            try
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
            catch (System.Exception ex)
            {

                throw ex;
            }
        }

        public async Task<bool> SendPhysicalRewardEmail(string to, string toName, string subject, PhysicalRewardEmail emailProps, CancellationToken cancellationToken)
        {
            try
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
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
    }
}
