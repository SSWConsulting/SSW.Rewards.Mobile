using System.Security.Claims;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.WebAPI.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId() => GetUser()?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public string GetUserEmail() => GetUser()?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value?.ToLowerInvariant()?.Trim() ?? string.Empty;

    public string GetUserFullName()
    {
        ClaimsPrincipal? user = GetUser();
        return $"{user?.FindFirstValue(ClaimTypes.GivenName)} {user?.FindFirstValue(ClaimTypes.Surname)}";
    }

    public string? GetUserProfilePic()
    {
        // TODO: Get the user profile pic from claims
        return null;
    }

    public bool IsInRole(string role) => GetUser()?.IsInRole(role) ?? false;

    private ClaimsPrincipal? GetUser() => _httpContextAccessor.HttpContext?.User;
}
