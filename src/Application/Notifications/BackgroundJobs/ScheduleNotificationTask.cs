using SSW.Rewards.Application.Notifications.Commands;

namespace SSW.Rewards.Application.Notifications.BackgroundJobs;

public class ScheduleNotificationTask(IMediator mediator)
{
    public async Task ProcessScheduledNotification(SendAdminNotificationCommand command)
    {
        if (mediator == null)
        {
            // This should not happen if DI is configured correctly with Hangfire
            throw new InvalidOperationException("IMediator was not initialized. Check Hangfire DI configuration.");
        }

        // Clear the schedule time as the job is now processing.
        command.ScheduleAt = null;
        await mediator.Send(command);
    }
}
