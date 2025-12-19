using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Posts;

namespace SSW.Rewards.ApiClient.Services;

public interface IPostsService
{
    Task<PostListViewModel> GetPosts(int page, int pageSize, bool publishedOnly, string? searchTerm, string? sortBy, string? sortDirection, CancellationToken cancellationToken);
    Task<PostDetailDto> GetPostById(int id, CancellationToken cancellationToken);
    Task<bool> ToggleLike(int postId, CancellationToken cancellationToken);
    Task<int> AddComment(int postId, string comment, CancellationToken cancellationToken);
    Task DeletePost(int postId, CancellationToken cancellationToken);
    Task<int> CreatePost(CreatePostDto dto, CancellationToken cancellationToken);
    Task UpdatePost(UpdatePostDto dto, CancellationToken cancellationToken);
    Task<string> UploadImage(byte[] fileBytes, string fileName, string contentType, CancellationToken cancellationToken);
}

public class PostsService : IPostsService
{
    private readonly HttpClient _httpClient;
    private const string _baseRoute = "api/Posts/";

    public PostsService(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<PostListViewModel> GetPosts(int page, int pageSize, bool publishedOnly, string? searchTerm, string? sortBy, string? sortDirection, CancellationToken cancellationToken)
    {
        var searchParam = string.IsNullOrEmpty(searchTerm) ? "" : $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        var sortByParam = string.IsNullOrEmpty(sortBy) ? "" : $"&sortBy={Uri.EscapeDataString(sortBy)}";
        var sortDirParam = string.IsNullOrEmpty(sortDirection) ? "" : $"&sortDirection={sortDirection}";
        var result = await _httpClient.GetAsync(
            $"{_baseRoute}GetPosts?page={page}&pageSize={pageSize}&publishedOnly={publishedOnly}{searchParam}{sortByParam}{sortDirParam}",
            cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<PostListViewModel>(cancellationToken: cancellationToken);
            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get posts: {responseContent}");
    }

    public async Task<PostDetailDto> GetPostById(int id, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetPostById?id={id}", cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<PostDetailDto>(cancellationToken: cancellationToken);
            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get post: {responseContent}");
    }

    public async Task<bool> ToggleLike(int postId, CancellationToken cancellationToken)
    {
        var command = new { PostId = postId };
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}ToggleLike", command, cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
            return response;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to toggle like: {responseContent}");
    }

    public async Task<int> AddComment(int postId, string comment, CancellationToken cancellationToken)
    {
        var command = new
        {
            PostId = postId,
            Comment = comment
        };

        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}AddComment", command, cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
            return response;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to add comment: {responseContent}");
    }

    public async Task DeletePost(int postId, CancellationToken cancellationToken)
    {
        var result = await _httpClient.DeleteAsync($"{_baseRoute}DeletePost?id={postId}", cancellationToken);

        if (!result.IsSuccessStatusCode)
        {
            var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Failed to delete post: {responseContent}");
        }
    }

    public async Task<int> CreatePost(CreatePostDto dto, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}CreatePost", dto, cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            var postId = await result.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
            return postId;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to create post: {responseContent}");
    }

    public async Task UpdatePost(UpdatePostDto dto, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PutAsJsonAsync($"{_baseRoute}UpdatePost", dto, cancellationToken);

        if (!result.IsSuccessStatusCode)
        {
            var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Failed to update post: {responseContent}");
        }
    }

    public async Task<string> UploadImage(byte[] fileBytes, string fileName, string contentType, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        var result = await _httpClient.PostAsync($"{_baseRoute}UploadImage", content, cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            var imageUrl = await result.Content.ReadAsStringAsync(cancellationToken);
            return imageUrl.Trim('"'); // Remove quotes from JSON string
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to upload image: {responseContent}");
    }
}
