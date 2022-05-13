using MediatR;
using Microsoft.AspNetCore.Http;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.User.Commands.UpsertCurrentUser;
using SSW.Rewards.Application.User.Queries.GetCurrentUser;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.WebAPI.Services
{
	public class CurrentUserService : ICurrentUserService
    {
        private readonly IMediator _mediatr;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private CurrentUserViewModel _currentUser;

        public CurrentUserService(
            IMediator mediatr, 
            IHttpContextAccessor httpContextAccessor)
        {
            _mediatr = mediatr;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserId() => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string GetUserEmail() => _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

        public string GetUserFullName()
        {
            ClaimsPrincipal user = _httpContextAccessor.HttpContext?.User;
            return $"{user?.FindFirstValue(ClaimTypes.GivenName)} {user?.FindFirstValue(ClaimTypes.Surname)}";
        }

        public async Task<CurrentUserViewModel> GetCurrentUserAsync(CancellationToken cancellationToken)
        {
            if (_currentUser != null)
            {
                return _currentUser;
            }

            try
            {
                return await _mediatr.Send(new GetCurrentUserQuery(), cancellationToken);
            }
            catch (NotFoundException)
            {
            }

            await _mediatr.Send(new UpsertCurrentUserCommand(), cancellationToken);

            return _currentUser = await _mediatr.Send(new GetCurrentUserQuery(), cancellationToken);
        }

        public string GetUserProfilePic()
        {
			// TODO: Get the user profile pic from claims
			return null;
        }
    }
}
