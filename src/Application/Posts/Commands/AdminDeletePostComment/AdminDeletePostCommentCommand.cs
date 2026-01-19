using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Posts.Commands.AdminDeletePostComment;

public class AdminDeletePostCommentCommand : IRequest<Unit>
{
    public int CommentId { get; set; }
}

public class AdminDeletePostCommentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AdminDeletePostCommentCommand, Unit>
{
    public async Task<Unit> Handle(AdminDeletePostCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await context.PostComments
            .AsTracking()
            .TagWithContext("GetCommentForAdminDelete")
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null)
        {
            throw new NotFoundException(nameof(PostComment), request.CommentId);
        }

        context.PostComments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
