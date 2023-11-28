using System.Security.Claims;

using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.WebUI.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId() => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public string GetUserEmail() => _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;

    public string GetUserFullName()
    {
        ClaimsPrincipal user = _httpContextAccessor.HttpContext?.User;
        return $"{user?.FindFirstValue(ClaimTypes.GivenName)} {user?.FindFirstValue(ClaimTypes.Surname)}";
    }

    public string GetUserProfilePic()
    {
        // TODO: Get the user profile pic from claims
        return null;
    }
}
