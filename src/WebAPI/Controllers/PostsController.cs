using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Posts.Commands.AddPostComment;
using SSW.Rewards.Application.Posts.Commands.CreatePost;
using SSW.Rewards.Application.Posts.Commands.DeletePost;
using SSW.Rewards.Application.Posts.Commands.DeletePostComment;
using SSW.Rewards.Application.Posts.Commands.TogglePostLike;
using SSW.Rewards.Application.Posts.Commands.UpdatePost;
using SSW.Rewards.Application.Posts.Queries.GetPostById;
using SSW.Rewards.Application.Posts.Queries.GetPosts;
using SSW.Rewards.Shared.DTOs.Posts;

namespace SSW.Rewards.WebAPI.Controllers;

public class PostsController : ApiControllerBase
{
    private readonly IPostImageStorageProvider _postImageStorageProvider;

    public PostsController(IPostImageStorageProvider postImageStorageProvider)
    {
        _postImageStorageProvider = postImageStorageProvider;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PostListViewModel), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<PostListViewModel>> GetPosts(
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool publishedOnly = true,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetPostsQuery
        {
            Page = page,
            PageSize = pageSize,
            PublishedOnly = publishedOnly,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        return Ok(await Mediator.Send(query));
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PostDetailDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<PostDetailDto>> GetPostById([FromQuery][Required] int id)
    {
        return Ok(await Mediator.Send(new GetPostByIdQuery { Id = id }));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<int>> CreatePost([FromBody][Required] CreatePostDto dto)
    {
        var command = new CreatePostCommand
        {
            Title = dto.Title,
            Content = dto.Content,
            ImageUrl = dto.ImageUrl,
            SendNotification = dto.SendNotification,
            IsPublished = dto.IsPublished
        };

        var postId = await Mediator.Send(command);
        return Ok(postId);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> UpdatePost([FromBody][Required] UpdatePostDto dto)
    {
        var command = new UpdatePostCommand
        {
            Id = dto.Id,
            Title = dto.Title,
            Content = dto.Content,
            ImageUrl = dto.ImageUrl,
            IsPublished = dto.IsPublished
        };

        await Mediator.Send(command);
        return Ok();
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeletePost([FromQuery][Required] int id)
    {
        await Mediator.Send(new DeletePostCommand { Id = id });
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<bool>> ToggleLike([FromBody][Required] TogglePostLikeCommand command)
    {
        var isLiked = await Mediator.Send(command);
        return Ok(isLiked);
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<int>> AddComment([FromBody][Required] AddPostCommentCommand command)
    {
        var commentId = await Mediator.Send(command);
        return Ok(commentId);
    }

    [HttpDelete]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    public async Task<ActionResult> DeleteComment([FromQuery][Required] int commentId)
    {
        await Mediator.Send(new DeletePostCommentCommand { CommentId = commentId });
        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<string>> UploadImage([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Invalid file type. Only image files are allowed.");
        }

        // Validate file size (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest("File size cannot exceed 5MB");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var imageArray = memoryStream.ToArray();

        var imageUrl = await _postImageStorageProvider.UploadPostImage(imageArray, file.FileName);

        return Ok(imageUrl);
    }
}
