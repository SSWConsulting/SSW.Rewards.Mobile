using Microsoft.AspNetCore.Http;
using SSW.Rewards.Application.Common.Interfaces;
using System.Linq;
using System.Security.Claims;

namespace SSW.Rewards.WebAPI.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserId() => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string GetUserEmail() => _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

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
}
