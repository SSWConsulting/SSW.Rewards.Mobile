using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Posts.Commands.DeletePostComment;

public class DeletePostCommentCommand : IRequest<Unit>
{
    public int CommentId { get; set; }
}

public class DeletePostCommentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<DeletePostCommentCommand, Unit>
{
    public async Task<Unit> Handle(DeletePostCommentCommand request, CancellationToken cancellationToken)
    {
        var userEmail = currentUserService.GetUserEmail();

        var user = await context.Users
            .AsNoTracking()
            .TagWithContext("GetUserForCommentDelete")
            .FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        var comment = await context.PostComments
            .AsTracking()
            .TagWithContext("GetCommentForDelete")
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null)
        {
            throw new NotFoundException(nameof(PostComment), request.CommentId);
        }

        // Check if user owns the comment or is an admin
        var isAdmin = currentUserService.IsInRole("Admin");
        if (comment.UserId != user.Id && !isAdmin)
        {
            throw new ForbiddenAccessException();
        }

        context.PostComments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
