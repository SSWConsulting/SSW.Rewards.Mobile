using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Shared.DTOs.Posts;

namespace SSW.Rewards.Application.Posts.Queries.GetPostById;

public class GetPostByIdQuery : IRequest<PostDetailDto>
{
    public int Id { get; set; }
}

public class GetPostByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<GetPostByIdQuery, PostDetailDto>
{
    public async Task<PostDetailDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var userEmail = currentUserService.GetUserEmail();
        var isAdmin = currentUserService.IsInRole("Admin");

        var user = await context.Users
            .AsNoTracking()
            .TagWithContext("GetCurrentUserForPostDetail")
            .FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);

        var currentUserId = user?.Id ?? 0;

        var post = await context.Posts
            .AsNoTracking()
            .TagWithContext("GetPostDetail")
            .Where(p => p.Id == request.Id)
            .Select(p => new PostDetailDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                SendNotification = p.SendNotification,
                IsPublished = p.IsPublished,
                PublishedDateUtc = p.PublishedDateUtc,
                CreatedUtc = p.CreatedUtc,
                LastModifiedUtc = p.LastModifiedUtc,
                LikesCount = p.PostLikes.Count,
                CommentsCount = p.PostComments.Count,
                CurrentUserLiked = user != null && p.PostLikes.Any(pl => pl.UserId == user.Id),
                Comments = p.PostComments
                    .OrderByDescending(c => c.CreatedUtc)
                    .Select(c => new PostCommentDto
                    {
                        Id = c.Id,
                        PostId = c.PostId,
                        UserId = c.UserId,
                        UserName = c.User.FullName ?? "",
                        UserAvatar = c.User.Avatar ?? "",
                        Comment = c.Comment,
                        CreatedUtc = c.CreatedUtc,
                        CanDelete = isAdmin || c.UserId == currentUserId
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
        {
            throw new NotFoundException(nameof(Post), request.Id);
        }

        return post;
    }
}
