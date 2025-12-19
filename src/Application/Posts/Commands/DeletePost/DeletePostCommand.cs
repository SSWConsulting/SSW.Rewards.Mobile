using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Posts.Commands.DeletePost;

public class DeletePostCommand : IRequest<Unit>
{
    public int Id { get; set; }
}

public class DeletePostCommandHandler(IApplicationDbContext context) : IRequestHandler<DeletePostCommand, Unit>
{
    public async Task<Unit> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await context.Posts
            .AsTracking()
            .TagWithContext("GetPostForDelete")
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), request.Id);
        }

        context.Posts.Remove(post);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
