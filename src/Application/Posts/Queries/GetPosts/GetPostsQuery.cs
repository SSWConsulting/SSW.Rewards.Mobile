using SSW.Rewards.Shared.DTOs.Posts;

namespace SSW.Rewards.Application.Posts.Queries.GetPosts;

public class GetPostsQuery : IRequest<PostListViewModel>, IPagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool PublishedOnly { get; set; } = false;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "desc";
}

public class GetPostsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<GetPostsQuery, PostListViewModel>
{
    public async Task<PostListViewModel> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var userEmail = currentUserService.GetUserEmail();

        var user = await context.Users
            .AsNoTracking()
            .TagWithContext("GetCurrentUserForPosts")
            .FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);

        var query = context.Posts
            .AsNoTracking()
            .TagWithContext("GetPosts")
            .Where(p => !request.PublishedOnly || p.IsPublished)
            .Where(p => string.IsNullOrEmpty(request.SearchTerm) ||
                       p.Title.Contains(request.SearchTerm) ||
                       p.Content.Contains(request.SearchTerm))

            .Select(p => new PostDto
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
                CurrentUserLiked = user != null && p.PostLikes.Any(pl => pl.UserId == user.Id)
            });

        // Apply sorting
        var sortedQuery = ApplySorting(query, request.SortBy, request.SortDirection);

        return await sortedQuery.ToPaginatedResultAsync<PostListViewModel, PostDto>(request, cancellationToken);
    }

    private static IQueryable<PostDto> ApplySorting(IQueryable<PostDto> query, string? sortBy, string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "title" => isDescending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
            "publisheddateutc" => isDescending ? query.OrderByDescending(p => p.PublishedDateUtc ?? p.CreatedUtc) : query.OrderBy(p => p.PublishedDateUtc ?? p.CreatedUtc),
            "likescount" => isDescending ? query.OrderByDescending(p => p.LikesCount) : query.OrderBy(p => p.LikesCount),
            "commentscount" => isDescending ? query.OrderByDescending(p => p.CommentsCount) : query.OrderBy(p => p.CommentsCount),
            _ => query.OrderByDescending(p => p.PublishedDateUtc ?? p.CreatedUtc) // Default sort
        };
    }
}
