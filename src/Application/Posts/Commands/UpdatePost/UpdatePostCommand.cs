using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Posts.Commands.UpdatePost;

public class UpdatePostCommand : IRequest<Unit>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsPublished { get; set; }
}

public class UpdatePostCommandHandler(
    IApplicationDbContext context,
    IDateTime dateTime) : IRequestHandler<UpdatePostCommand, Unit>
{
    public async Task<Unit> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await context.Posts
            .AsTracking()
            .TagWithContext("GetPostForUpdate")
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), request.Id);
        }

        post.Title = request.Title;
        post.Content = request.Content;
        post.ImageUrl = request.ImageUrl;

        // If changing from unpublished to published, set the published date
        if (request.IsPublished && !post.IsPublished)
        {
            post.PublishedDateUtc = dateTime.Now;
        }

        post.IsPublished = request.IsPublished;

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
