using Microsoft.Extensions.Logging;
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

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly ISender _sender;
    private readonly ILogger<CreatePostCommandHandler> _logger;

    public CreatePostCommandHandler(
        IApplicationDbContext context,
        IDateTime dateTime,
        ISender sender,
        ILogger<CreatePostCommandHandler> logger)
    {
        _context = context;
        _dateTime = dateTime;
        _sender = sender;
        _logger = logger;
    }

    public async Task<int> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            ImageUrl = request.ImageUrl,
            SendNotification = request.SendNotification,
            IsPublished = request.IsPublished,
            PublishedDateUtc = request.IsPublished ? _dateTime.Now : null
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync(cancellationToken);

        // Send push notification if requested and post is published
        if (request.SendNotification && request.IsPublished)
        {
            try
            {
                var command = new Notifications.Commands.SendAdminNotificationCommand
                {
                    Title = post.Title,
                    Body = post.Content,
                    ImageUrl = post.ImageUrl,
                };
                await _sender.Send(command, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification for new post {PostId}", post.Id);
            }
        }

        return post.Id;
    }
}
