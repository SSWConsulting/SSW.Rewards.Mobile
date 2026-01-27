
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Application.Posts.Commands.UpdatePost;

public class UpdatePostCommand : IRequest<Unit>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public bool SendNotification { get; set; }
}

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly ISender _sender;
    private readonly ILogger<UpdatePostCommandHandler> _logger;

    public UpdatePostCommandHandler(
        IApplicationDbContext context,
        IDateTime dateTime,
        ISender sender,
        ILogger<UpdatePostCommandHandler> logger)
    {
        _context = context;
        _dateTime = dateTime;
        _sender = sender;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .AsTracking()
            .TagWithContext("GetPostForUpdate")
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), request.Id);
        }

        var wasDraft = !post.IsPublished;

        post.Title = request.Title;
        post.Content = request.Content;
        post.ImageUrl = request.ImageUrl;

        // If changing from unpublished to published, set the published date
        if (request.IsPublished && !post.IsPublished)
        {
            post.PublishedDateUtc = _dateTime.Now;
        }

        post.IsPublished = request.IsPublished;

        await _context.SaveChangesAsync(cancellationToken);

        // If publishing a draft and SendNotification is true, send notification
        if (wasDraft && request.IsPublished && request.SendNotification)
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
                _logger.LogError(ex, "Failed to send notification for published post {PostId}", post.Id);
            }
        }

        return Unit.Value;
    }
}
