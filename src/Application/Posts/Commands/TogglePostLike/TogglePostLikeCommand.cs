namespace SSW.Rewards.Application.Posts.Commands.TogglePostLike;

public class TogglePostLikeCommand : IRequest<bool>
{
    public int PostId { get; set; }
}

public class TogglePostLikeCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<TogglePostLikeCommand, bool>
{
    public async Task<bool> Handle(TogglePostLikeCommand request, CancellationToken cancellationToken)
    {
        var userEmail = currentUserService.GetUserEmail();

        var user = await context.Users
            .AsNoTracking()
            .TagWithContext("GetUserForLike")
            .FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        var existingLike = await context.PostLikes
            .AsTracking()
            .TagWithContext("CheckExistingLike")
            .FirstOrDefaultAsync(pl => pl.PostId == request.PostId && pl.UserId == user.Id, cancellationToken);

        if (existingLike != null)
        {
            // Unlike - remove the like
            context.PostLikes.Remove(existingLike);
            await context.SaveChangesAsync(cancellationToken);
            return false; // Not liked anymore
        }
        else
        {
            // Like - add the like
            var postLike = new PostLike
            {
                PostId = request.PostId,
                UserId = user.Id
            };
            context.PostLikes.Add(postLike);
            await context.SaveChangesAsync(cancellationToken);
            return true; // Now liked
        }
    }
}
