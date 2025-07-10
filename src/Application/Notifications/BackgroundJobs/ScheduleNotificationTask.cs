using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Notifications.Commands;

namespace SSW.Rewards.Application.Notifications.BackgroundJobs;

public class ScheduleNotificationTask(IMediator mediator, ILogger<ScheduleNotificationTask> logger)
{
    public async Task ProcessScheduledNotification(SendAdminNotificationCommand command)
    {
        if (mediator == null)
        {
            // This should not happen if DI is configured correctly with Hangfire
            throw new InvalidOperationException("IMediator was not initialized. Check Hangfire DI configuration.");
        }
        
        try
        {
            await mediator.Send(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing scheduled notification: {Title}", command.Title);
            throw;
        }
    }
}
