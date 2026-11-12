using SSW.Rewards.ApiClient.Paging;
using SSW.Rewards.Shared.DTOs.Posts;

namespace SSW.Rewards.ApiClient.Services;

public static class PostsServiceExtensions
{
    /// <summary>
    /// One skip/take page of published posts, newest first: the mobile Posts tab's fetch.
    /// Shared with the integration tests so they exercise the same request.
    /// </summary>
    public static async Task<List<PostDto>> GetPublishedPostsAsync(
        this IPostsService posts, int skip, int take, CancellationToken cancellationToken)
    {
        var result = await posts.GetPosts(
            PageNumberPaging.ToPageIndex(skip, take), take, publishedOnly: true,
            searchTerm: null, sortBy: null, sortDirection: null, cancellationToken);
        return result.Items?.ToList() ?? [];
    }
}
