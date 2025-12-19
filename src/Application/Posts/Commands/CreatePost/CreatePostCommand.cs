using SSW.Rewards.Application.Common.Models;

namespace SSW.Rewards.Application.Posts.Commands.CreatePost;

public class CreatePostCommand : IRequest<int>
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool SendNotification { get; set; }
    public bool IsPublished { get; set; }
}

public class CreatePostCommandHandler(
    IApplicationDbContext context,
    IDateTime dateTime,
    INotificationService notificationService,
    IUserService userService) : IRequestHandler<CreatePostCommand, int>
{
    public async Task<int> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            ImageUrl = request.ImageUrl,
            SendNotification = request.SendNotification,
            IsPublished = request.IsPublished,
            PublishedDateUtc = request.IsPublished ? dateTime.Now : null
        };

        context.Posts.Add(post);
        await context.SaveChangesAsync(cancellationToken);

        // Send push notification if requested and post is published
        if (request.SendNotification && request.IsPublished)
        {
            var notification = new NotificationRequest
            {
                Text = request.Title,
                Action = $"post:{post.Id}",
                Tags = Array.Empty<string>(), // Send to all users
                Silent = false
            };

            await notificationService.RequestNotificationAsync(notification, cancellationToken);

            // Save notification history
            var userId = await userService.GetCurrentUserId(cancellationToken);
            var notificationEntry = new Notification
            {
                Message = notification.Text,
                NotificationTag = string.Join(",", notification.Tags),
                NotificationAction = notification.Action,
                SentByStaffMemberId = userId
            };

            context.Notifications.Add(notificationEntry);
            await context.SaveChangesAsync(cancellationToken);
        }

        return post.Id;
    }
}
