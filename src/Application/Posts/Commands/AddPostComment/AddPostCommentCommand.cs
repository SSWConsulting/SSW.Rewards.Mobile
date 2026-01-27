namespace SSW.Rewards.Application.Posts.Commands.AddPostComment;

public class AddPostCommentCommand : IRequest<int>
{
    public int PostId { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class AddPostCommentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<AddPostCommentCommand, int>
{
    public async Task<int> Handle(AddPostCommentCommand request, CancellationToken cancellationToken)
    {
        var userEmail = currentUserService.GetUserEmail();

        var user = await context.Users
            .AsNoTracking()
            .TagWithContext("GetUserForComment")
            .FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        var postComment = new PostComment
        {
            PostId = request.PostId,
            UserId = user.Id,
            Comment = request.Comment
        };

        context.PostComments.Add(postComment);
        await context.SaveChangesAsync(cancellationToken);

        return postComment.Id;
    }
}
